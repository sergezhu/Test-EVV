namespace Code.MergeSystem
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Code.Core;
	using Code.Input.Touch;
	using UniRx;
	using UnityEngine;
	using Utilities.Extensions;

	public struct MergeCellInfo
	{
		public Vector2Int Coord;
		public int CellIndex;
		public int MergeLevel;
		public Vector3 WorldPosition;
	}

	public enum MergeOperation
	{
		MergeOnBoard,
		EquipWithMerge,
		EquipWithoutMerge
	}

	public class MergeBoardController : IInitializable
	{
		private readonly MergeBoardView _view;
		private readonly MergeConfig _mergeConfig;
		private readonly ITouchInput _touchInput;
		private readonly BoardState _boardState;
		private readonly MergeItemFactory _factory;
		private readonly CompositeDisposable _disposable;
		private readonly CompositeDisposable _touchDisposable;
		private readonly CompositeDisposable _itemsDisposable;

		private MergeItem[,] _itemsMatrix;
		private List<Vector2Int> _otherCanBeMergedItemsCoords;
		private TouchData _lastTouchData;
		private MergeItem _draggedItem;
		private MergeBoardCellView _currentCellView;
		private MergeItemReceiver _receiverUnderPointer;

		private readonly RaycastHit[] _hits;
		private MergeOperation[] _allOperations;
		private List<MergeOperation> _allowedOperations;

		public MergeBoardController(MergeBoardView view, MergeItemFactory factory, MergeConfig mergeConfig, 
									ITouchInput touchInput, BoardState boardState)
		{
			_view = view;
			_mergeConfig = mergeConfig;
			_touchInput = touchInput;
			_boardState = boardState;
			_factory = factory;

			_hits = new RaycastHit[10];
			_itemsMatrix = new MergeItem[_mergeConfig.BoardSize.x, _mergeConfig.BoardSize.y];
			_otherCanBeMergedItemsCoords = new List<Vector2Int>();
			
			_disposable = new CompositeDisposable();
			_touchDisposable = new CompositeDisposable();
			_itemsDisposable = new CompositeDisposable();
		}


		private bool IsLocked { get; set; }
		private bool IsMergeProcessing { get; set; }
		public bool HasEmptyCell => HasFreePosition();
		public ReactiveCommand<int> IsMergeItemCreatedFirst { get; } = new ReactiveCommand<int>();
		public ReactiveCommand<int> IsItemsMergedOnBoard { get; } = new ReactiveCommand<int>();
		public ReactiveCommand<int> IsItemEquippedWithMerge { get; } = new ReactiveCommand<int>();
		public ReactiveCommand<int> IsItemEquippedWithoutMerge { get; } = new ReactiveCommand<int>();

		public void Initialize()
		{
			_allOperations = new MergeOperation[] { MergeOperation.EquipWithMerge, MergeOperation.EquipWithoutMerge, MergeOperation.MergeOnBoard };
		}

		public void Activate()
		{
			PostInitialize();
		}
		
		public void Deactivate()
		{
			CleanUp();
		}


		public void BuyItem()
		{
			var mergeLevel = _boardState.CurrentMergeLevel;
			CreateItemAtRandomPosition(mergeLevel);

			_boardState.AddBuyedToStats( mergeLevel );
			
			CheckIfItemCreatedFirst( mergeLevel );
		}

		public void TryUpgradeAllItemsToCurrentLevel()
		{
			for ( var x = 0; x < _mergeConfig.BoardSize.x; x++ )
			for ( var y = 0; y < _mergeConfig.BoardSize.y; y++ )
			{
				if ( _itemsMatrix[x, y] == null )
					continue;

				TryUpgradeItemToCurrentLevel( new Vector2Int( x, y ) );
			}
		}

		public void Lock()
		{
			IsLocked = true;
			ClearDragging();
		}

		public void Unlock()
		{
			IsLocked = false;
		}

		public void AllowAllOperations()
		{
			AllowOperations( _allOperations );
		}

		public void DenyAllOperations()
		{
			if ( _allowedOperations == null )
			{
				_allowedOperations = new List<MergeOperation>();
			}
			else
			{
				_allowedOperations.Clear();
			}
		}

		public void AllowOperations( IEnumerable<MergeOperation> operations )
		{
			_allowedOperations = new List<MergeOperation>();
			operations.ForEach( op => _allowedOperations.Add( op ) );
		}

		public List<MergeCellInfo> GetCellsWithItemsInfo()
		{
			var result = new List<MergeCellInfo>();

			for ( var x = 0; x < _mergeConfig.BoardSize.x; x++ )
			for ( var y = 0; y < _mergeConfig.BoardSize.y; y++ )
			{
				if ( _itemsMatrix[x, y] == null )
					continue;

				var level = _itemsMatrix[x, y].MergeLevel;
				var cellIndex = MatrixIndexToCellIndex( new Vector2Int( x, y ), _mergeConfig.BoardSize.x );
				var cellPos = _view.GetSpawnPosition( cellIndex );
				
				result.Add( new MergeCellInfo()
				{
					CellIndex =  cellIndex,
					Coord = new Vector2Int( x, y ),
					MergeLevel = level,
					WorldPosition = cellPos
				} );
			}

			return result;
		}

		public bool TryGetFirstMergePair( out MergeCellInfo from, out MergeCellInfo to )
		{
			var cellsInfos = GetCellsWithItemsInfo();

			from = default;
			to = default;

			foreach ( var info in cellsInfos )
			{
				var sameLevelItemsCount = cellsInfos.Count( i => i.MergeLevel == info.MergeLevel );

				if ( sameLevelItemsCount >= 2 )
				{
					var infos = cellsInfos.Where( i => i.MergeLevel == info.MergeLevel ).ToArray();
					from = infos[0];
					to = infos[1];
					return true;
				}
			}

			return false;
		}

		public bool TryGetFirstRecordForEquipWithMerge( MergeItemReceiver receiver, out MergeCellInfo from )
		{
			from = default;

			if ( receiver.HasItem == false )
				return false;
			
			var cellsInfos = GetCellsWithItemsInfo();
			var fromIndex = cellsInfos.FindIndex( info => info.MergeLevel == receiver.GetCurrentMergeLevel() );

			if ( fromIndex == -1 )
				return false;

			from = cellsInfos[fromIndex];
			
			return true;
		}

		public bool TryGetFirstRecordForEquipWithoutMerge( MergeItemReceiver receiver, out MergeCellInfo from )
		{
			from = default;
			var cellsInfos = GetCellsWithItemsInfo();

			if ( receiver.HasItem == false )
			{
				var hasItems = cellsInfos.Count > 0;

				if ( hasItems == false )
					return false;

				from = cellsInfos[0];
				return true;
			}

			var fromIndex = cellsInfos.FindIndex( info => info.MergeLevel != receiver.GetCurrentMergeLevel() );
			
			if ( fromIndex == -1 )
				return false;

			from = cellsInfos[fromIndex];

			return true;
		}

		private void PostInitialize()
		{
			_view.Show();

			AllowAllOperations();
			CreateStartMergeItems();
			SubscribeMergeItems();
			SubscribeTouch();
		}

		private void CleanUp()
		{
			if ( _draggedItem != null )
			{
				_draggedItem.SetDefaultItemLayer();
				_draggedItem.Rollback();
				_draggedItem = null;
			}

			SaveRestoreData();
			
			_view.Hide();

			UnsubscribeTouch();
			UnsubscribeMergeItems();
			DisableCellsHints();
			DetroyMergeItems();
		}

		private void ResubscribeMergeItems()
		{
			UnsubscribeMergeItems();
			SubscribeMergeItems();
		}

		private void SubscribeMergeItems()
		{
			for ( var x = 0; x < _mergeConfig.BoardSize.x; x++ )
			for ( var y = 0; y < _mergeConfig.BoardSize.y; y++ )
			{
				if ( _itemsMatrix[x, y] == null )
					continue;

				var item = _itemsMatrix[x, y];
				
				//item.***.Subscribe
			}
		}

		private void SubscribeTouch()
		{
			_touchInput.OnTouchStart
				.Where( _ => IsLocked == false )
				.Subscribe( pos => OnTouchStart( pos ) )
				.AddTo( _touchDisposable );

			_touchInput.OnTouchEnd
				.Where( _ => IsLocked == false )
				.Subscribe( pos => OnTouchEnd( pos ) )
				.AddTo( _touchDisposable );

			_touchInput.OnTouchPositionChanged
				.Where( _ => IsLocked == false )
				.Subscribe( pos => OnTouchPositionChanged( pos ) )
				.AddTo( _touchDisposable );
		}

		private void ClearDragging()
		{
			if ( _draggedItem != null )
			{
				_draggedItem.SetDefaultItemLayer();
				_draggedItem.Rollback();
			}

			CrearDraggingVariables();
		}

		private void OnTouchStart( TouchData data )
		{
			if ( IsMergeProcessing )
				return;
			
			_lastTouchData = data;

			var itemViewHit = ThrowRaycastAndGetNearestItem();

			if ( itemViewHit != null )
				itemViewHit.OnTouchStartHit();
			
			Debug.Log( $"OnTouchStart : {data.WorldDirectionFromMergeCamera}, is hit : {itemViewHit != null}" );
			Debug.DrawLine( data.MergeCameraPosition, data.MergeCameraPosition + data.WorldDirectionFromMergeCamera.normalized * _mergeConfig.RaycastDistance, Color.red, 2f );

			var item = FindItemWithStartHit();
			_draggedItem = item;

			if ( _draggedItem != null )
			{
				_draggedItem.SetDraggedItemLayer();
				UpdateOtherCellsCoordsCanBeMerged();
				UpdateCellsHints();
			}
		}

		private void OnTouchEnd( TouchData data )
		{
			Debug.DrawLine( data.MergeCameraPosition, data.MergeCameraPosition + data.WorldDirectionFromMergeCamera.normalized * _mergeConfig.RaycastDistance, Color.red, 2f );

			//var receiver = ThrowRaycastAndGetItemReceiver();
			FindCellOrReceiver( out var cellView, out var receiver );
			
			Debug.Log( $"OnTouchEnd : {data.WorldDirectionFromMergeCamera}, receiver is null : {receiver == null}, cellView is null : {cellView == null}" );

			if ( receiver != null && _draggedItem != null )
			{
				/*if ( receiver.CanSwapItem( _draggedItem.DbInfo ) && _allowedOperations.Contains( MergeOperation.EquipWithoutMerge ))
				{
					var receiverItem = receiver.CurrentItem.Value;
					var receiverItemLevel = _mergeConfig.GetMergeLevel( receiverItem.ID );
					var draggedItemCoord = FindItemCoords( _draggedItem );

					receiver.ReceiveItem( _draggedItem.DbInfo, new MergeItemReceiveOptions(){CanPlayReceiveFX = true} );
					DestroyItem( _draggedItem );

					if ( receiverItemLevel != -1 )
					{
						CreateItemAtPosition( draggedItemCoord, receiverItemLevel );
					}

					//Debug.Log( "-- set _draggedItem as null in CanSwapItem checking" );

					SaveRestoreData();
					
					IsItemEquippedWithoutMerge.Execute( _draggedItem.MergeLevel );
					
					_draggedItem = null;

					return;
				}*/
				
				if ( receiver.CanMergeItem( _draggedItem.DbInfo ) && 
					 _mergeConfig.HasNextMergeLevel( _draggedItem.DbInfo.ID ) && 
					 _allowedOperations.Contains( MergeOperation.EquipWithMerge ) )
				{
					var nextLevel = _mergeConfig.GetNextMergeLevel( _draggedItem.DbInfo.ID );
					var nextItem = _mergeConfig.GetMergeItem( nextLevel );
					receiver.ReceiveItem( nextItem, new MergeItemReceiveOptions() { CanPlayReceiveFX = true } );
					DestroyItem( _draggedItem );

					//Debug.Log( "-- set _draggedItem as null in CanMergeItem checking" );

					SaveRestoreData();
					
					IsItemEquippedWithMerge.Execute( nextLevel );
					
					_draggedItem = null;
					
					_boardState.AddMergedToStats( nextLevel );
					
					CheckIfItemCreatedFirst( nextLevel );
					return;
				}
			}
			
			/*var itemViewHit = ThrowRaycastAndGetNearestItem();

			if ( itemViewHit != null )
				itemViewHit.OnTouchEndHit(); */

			if ( cellView != null )
			{
				var canMergeToCell = CanMergeToCell( cellView, out var cellIndex, out var matrixPos);

				if ( canMergeToCell && _allowedOperations.Contains( MergeOperation.MergeOnBoard ) )
				{
					var item = _itemsMatrix[matrixPos.x, matrixPos.y];
					DoMerge( _draggedItem, item );
					DestroyItem( _draggedItem );
					_draggedItem = null;
				}
				else
				{
					TryRollbackItem( _draggedItem );
				}
			}
			else
			{
				TryRollbackItem( _draggedItem );
			}


			//var item = FindItemWithEndHit();
			//DoMergeOrRollback( _draggedItem, item );
			
			DisableCellsHints();
			CrearDraggingVariables();
		}

		private void OnTouchPositionChanged( TouchData data )
		{
			if ( IsMergeProcessing )
				return;
			
			_lastTouchData = data;
			
			if(_draggedItem == null)
				return;

			//Debug.DrawLine( data.WorldProjectionFromMergeCamera, data.WorldProjectionFromMergeCamera + _view.transform.up * _mergeConfig.DraggedItemOffsetY, Color.red, 0.5f );
			_draggedItem.SetPosition( data.WorldProjectionFromMergeCamera + _view.transform.up * _mergeConfig.DraggedItemOffsetY );

			UpdateCellsHints();
		}

		private void UpdateOtherCellsCoordsCanBeMerged()
		{
			var mergeLevel = _draggedItem.MergeLevel;

			for ( var x = 0; x < _mergeConfig.BoardSize.x; x++ )
			for ( var y = 0; y < _mergeConfig.BoardSize.y; y++ )
			{
				var mergeItem = _itemsMatrix[x, y];
				
				if ( mergeItem == null )
					continue;

				if ( mergeItem != _draggedItem && mergeItem.MergeLevel == mergeLevel )
				{
					_otherCanBeMergedItemsCoords.Add( new Vector2Int( x, y ) );
				}
			}
		}


		/*private void DoMergeOrRollback( MergeItem fromItem, MergeItem toItem )
		{
			if ( HasSameLevelAndExistNextLevel( fromItem, toItem ) )
			{
				DoMerge( fromItem, toItem );
			}
			else
			{
				TryRollbackItem( fromItem );
			}
		}*/

		private void DoMerge( MergeItem fromItem, MergeItem toItem )
		{
			Debug.Log( "MERGE" );

			IsMergeProcessing = true;
			
			var nextMergeLevel = toItem.MergeLevel + 1;
			var toItemCoord = FindItemCoords( toItem );

			fromItem.PlayHideAnimation( () => { DestroyItem( fromItem ); } );
			
			toItem.PlayHideAnimation( () =>
			{
				DestroyItem( toItem );
				CreateItemAt( toItemCoord, nextMergeLevel );
				ResubscribeMergeItems();
				
				_boardState.AddMergedToStats( nextMergeLevel );

				var item = _itemsMatrix[toItemCoord.x, toItemCoord.y];
				item.PlayMergeFX();
				item.PlayShowAnimation( null );

				CheckIfItemCreatedFirst( nextMergeLevel );

				IsMergeProcessing = false;

				SaveRestoreData();
				IsItemsMergedOnBoard.Execute( nextMergeLevel );
			} );
		}

		private void TryRollbackItem( MergeItem fromItem )
		{
			if ( fromItem != null )
			{
				fromItem.SetDefaultItemLayer();
				fromItem.Rollback();
			}
		}

		private void CrearDraggingVariables()
		{
			_draggedItem = null;
			_currentCellView = null;
			_otherCanBeMergedItemsCoords.Clear();
		}

		private bool HasSameLevelAndExistNextLevel( MergeItem fromItem, MergeItem toItem )
		{
			var isItemsValid = fromItem != null && toItem != null && fromItem != toItem && fromItem.MergeLevel == toItem.MergeLevel;

			if ( isItemsValid == false )
				return false;

			var hasNextMergeLevel = _mergeConfig.HasNextMergeLevel( toItem.DbInfo.ID );

			return hasNextMergeLevel;
		}

		private void CheckIfItemCreatedFirst( int mergeLevel )
		{
			var currentStats = _boardState.MergeStatistic.GetStatistic( mergeLevel );
			if ( currentStats != null && currentStats.Value.CreatedCount == 1 )
			{
				IsMergeItemCreatedFirst.Execute( mergeLevel );
			}
		}

		private void TryUpgradeItemToCurrentLevel( Vector2Int coord )
		{
			var fromItem = _itemsMatrix[coord.x, coord.y];
			var fromItemLevel = _mergeConfig.GetMergeLevel( fromItem.DbInfo.ID );
			var targetMergeLevel = _boardState.CurrentMergeLevel;
			var canUpgrade = fromItemLevel < targetMergeLevel;

			if ( canUpgrade )
			{
				fromItem.PlayHideAnimation( () =>
				{
					DestroyItem( fromItem );
					CreateItemAt( coord, targetMergeLevel );
					ResubscribeMergeItems();

					var item = _itemsMatrix[coord.x, coord.y];
					item.PlayMergeFX();
					item.PlayShowAnimation( null );

					_boardState.AddUpgradeCountToStats();
				} );
			}
		}

		private void DestroyItem( MergeItem item )
		{
			var firstItemCoord = FindItemCoords( item );
			DestroyItem( firstItemCoord );
		}

		private void DestroyItem( Vector2Int coord )
		{
			_itemsMatrix[coord.x, coord.y].DestroyView();
			_itemsMatrix[coord.x, coord.y].Dispose();
			_itemsMatrix[coord.x, coord.y] = null;
		}

		private void UpdateCellsHints()
		{
			FindCellOrReceiver( out var cellView, out var receiver );

			if ( _currentCellView == cellView || _currentCellView == null && cellView == null )
				return;
			
			//Debug.Log( "UpdateCellsHints : UpdateCellsHints" );

			var canMergeCurrent = CanMergeToCell( _currentCellView, out var currentCellIndex, out var currentCellCoord );
			//var currentCellIndex = _view.GetCellIndex( _currentCellView );
			//var currentCellCoord = CellIndexToMatrixIndex( currentCellIndex, _mergeConfig.BoardSize.x );
			var isCurrentOtherCanBeMerged = _otherCanBeMergedItemsCoords.Contains( currentCellCoord );
			var newCurrentState = isCurrentOtherCanBeMerged ? CellInteractionState.OtherMergeable : CellInteractionState.Default;
			_view.SwitchToState( _currentCellView, newCurrentState );
			

			var canMergeNext = CanMergeToCell( cellView, out var nextCellIndex, out var nextCellCoord );
			var newNextState = canMergeNext ? CellInteractionState.Success : CellInteractionState.Fail;
			_view.SwitchToState( cellView, newNextState );
			

			Debug.Log( $"UpdateCellsHints : UpdateCellsHints, canMerge {canMergeNext}, isCurrentOther {isCurrentOtherCanBeMerged}, curState : {newCurrentState}, nextState : {newNextState}" );
			
			/*if( canMerge )
			{
				_view.SwitchToState( cellView, CellInteractionState.Success );
			}
			else
			{
				_view.SwitchToState( cellView, CellInteractionState.Fail );
			}*/
			
			_currentCellView = cellView;
		}

		private bool CanMergeToCell( MergeBoardCellView cellView, out int cellIndex, out Vector2Int matrixIndex )
		{
			if ( cellView == null )
			{
				cellIndex = -1;
				matrixIndex = new Vector2Int( -1, -1 );
				return false;
			}
			
			cellIndex = _view.GetCellIndex( cellView );
			matrixIndex = CellIndexToMatrixIndex( cellIndex, _mergeConfig.BoardSize.x );

			for ( var i = 0; i < _otherCanBeMergedItemsCoords.Count; i++ )
			{
				var coord = _otherCanBeMergedItemsCoords[i];
				var isOther = coord != matrixIndex;

				if ( isOther == false )
					continue;

				var otherCellIndex = MatrixIndexToCellIndex( coord, _mergeConfig.BoardSize.x );
				_view.SwitchToState( otherCellIndex, CellInteractionState.OtherMergeable );
			}

			var attachedItem = _itemsMatrix[matrixIndex.x, matrixIndex.y];
			var hasAttachedItem = attachedItem != null;
			var canMerge = false;

			if ( hasAttachedItem )
			{
				canMerge = HasSameLevelAndExistNextLevel( _draggedItem, attachedItem );
			}

			return canMerge;
		}

		private void FindCellOrReceiver( out MergeBoardCellView cellView, out MergeItemReceiver receiver )
		{
			receiver = ThrowRaycastAndGetItemReceiver();
			cellView = ThrowRaycastAndGetNearestCell();

			if ( cellView == null )
			{
				if ( receiver != null )
				{
					_currentCellView = null;
				}

				if ( receiver != null && _receiverUnderPointer == null )
				{
					_receiverUnderPointer = receiver;
					DisableCellsHints();
				}

				if ( receiver == null && _receiverUnderPointer != null )
				{
					_receiverUnderPointer = receiver;
				}
			}
			else
			{
				_receiverUnderPointer = null;
			}
		}

		private TComponent ThrowRaycastAndGetNearestObject<TComponent>(Vector3 camPosition, Vector3 dirFromCam, int mask) where TComponent : Component
		{
			Physics.RaycastNonAlloc( camPosition, dirFromCam, _hits, _mergeConfig.RaycastDistance, mask );

			var components = _hits
				.Where( hit => hit.collider != null )
				.Select( hit => hit.collider.gameObject.GetComponent<TComponent>() )
				.ToList();

			TComponent firstComponent = null;

			if ( components.Count > 0 )
			{
				firstComponent = components[0];
			}

			return firstComponent;
		}
		
		private MergeItemView ThrowRaycastAndGetNearestItem()
		{
			var mask = (1 << _mergeConfig.MergeItemLayer);
			return ThrowRaycastAndGetNearestObject<MergeItemView>( _lastTouchData.MergeCameraPosition, _lastTouchData.WorldDirectionFromMergeCamera, mask );
		}

		private MergeBoardCellView ThrowRaycastAndGetNearestCell()
		{
			var mask = (1 << _mergeConfig.MergeBoardLayer);
			return ThrowRaycastAndGetNearestObject<MergeBoardCellView>( _lastTouchData.MergeCameraPosition, _lastTouchData.WorldDirectionFromMergeCamera, mask );
		}

		private MergeItemReceiver ThrowRaycastAndGetItemReceiver()
		{
			var mask = (1 << _mergeConfig.MergeItemReceiverLayer);
			return ThrowRaycastAndGetNearestObject<MergeItemReceiver>( _lastTouchData.MainCameraPosition, _lastTouchData.WorldDirectionFromMainCamera, mask );
		}

		private MergeItem FindItemWithStartHit()
		{
			for ( var x = 0; x < _mergeConfig.BoardSize.x; x++ )
			for ( var y = 0; y < _mergeConfig.BoardSize.y; y++ )
			{
				if ( _itemsMatrix[x, y] == null )
					continue;

				if ( _itemsMatrix[x, y].TouchStartFlag )
					return _itemsMatrix[x, y];
			}

			return null;
		}

		private MergeItem FindItemWithEndHit()
		{
			for ( var x = 0; x < _mergeConfig.BoardSize.x; x++ )
			for ( var y = 0; y < _mergeConfig.BoardSize.y; y++ )
			{
				if ( _itemsMatrix[x, y] == null )
					continue;

				if ( _itemsMatrix[x, y].TouchEndFlag )
					return _itemsMatrix[x, y];
			}

			return null;
		}

		private Vector2Int FindItemCoords( MergeItem item )
		{
			if ( item == null )
				throw new InvalidOperationException();
			
			for ( var x = 0; x < _mergeConfig.BoardSize.x; x++ )
			for ( var y = 0; y < _mergeConfig.BoardSize.y; y++ )
			{
				if ( _itemsMatrix[x, y] == null )
					continue;

				if ( _itemsMatrix[x, y] == item )
					return new Vector2Int( x, y );
			}

			return new Vector2Int( -1, -1 );
		}

		private void UnsubscribeTouch()
		{
			_touchDisposable.Clear();
		}

		private void UnsubscribeMergeItems()
		{
			_itemsDisposable.Clear();
		}

		private void CreateStartMergeItems()
		{
			//var restoredItemsData = GetRestoredItemsData();
			var restoredItemsData = _boardState.MergeBoardRestoreData;
			var canRestore = restoredItemsData.Count > 0;

			if ( canRestore )
			{
				restoredItemsData.ForEach( data =>
				{
					CreateItemAtPosition( new Vector2Int( data.X, data.Y ), data.MergeLevel );
				} );
			}
			else
			{
				for ( int i = 0; i < _mergeConfig.SpawnedItemsCountByStart; i++ )
				{
					CreateItemAtRandomPosition( _boardState.CurrentMergeLevel );
				}
			}
		}

		private List<MergeBoardCellRecord> GetRestoredItemsData()
		{
			var restoredItemsData = new List<MergeBoardCellRecord>();

			for ( var x = 0; x < _mergeConfig.BoardSize.x; x++ )
			for ( var y = 0; y < _mergeConfig.BoardSize.y; y++ )
			{
				if ( _itemsMatrix[x, y] == null )
					continue;

				restoredItemsData.Add( new MergeBoardCellRecord()
				{
					X = x,
					Y = y,
					MergeLevel = _itemsMatrix[x, y].MergeLevel
				} );
			}

			return restoredItemsData;
		}

		private void CreateItemAtRandomPosition(int mergeLevel )
		{
			var matrixPos = GetRandomFreePosition();
			CreateItemWithFX( matrixPos, mergeLevel );
		}

		private void CreateItemAtPosition( Vector2Int matrixPos, int mergeLevel )
		{
			CreateItemWithFX( matrixPos, mergeLevel );
		}

		private void CreateItemWithFX( Vector2Int matrixPos, int mergeLevel )
		{
			CreateItemAt( matrixPos, mergeLevel );

			var item = _itemsMatrix[matrixPos.x, matrixPos.y];
			item.PlaySpawnFX();
			item.PlayShowAnimation( null );
		}

		private void CreateItemAt( Vector2Int matrixPos, int mergeLevel )
		{
			var cellIndex = MatrixIndexToCellIndex( matrixPos, _mergeConfig.BoardSize.x );
			var worldPos = _view.GetSpawnPosition( cellIndex );
			var itemRoot = _view.GetItemRoot( cellIndex );
			_itemsMatrix[matrixPos.x, matrixPos.y] = _factory.Create( mergeLevel, worldPos, itemRoot );
		}

		private void DetroyMergeItems()
		{
			for ( var x = 0; x < _mergeConfig.BoardSize.x; x++ )
			for ( var y = 0; y < _mergeConfig.BoardSize.y; y++ )
			{
				if ( _itemsMatrix[x, y] == null )
					continue;

				DestroyItem( new Vector2Int( x, y ) );
			}
		}

		private void SaveRestoreData()
		{
			List<MergeBoardCellRecord> restoreData = GetRestoredItemsData();
			_boardState.UpdateRestoreData( restoreData );
		}


		private Vector2Int GetRandomFreePosition()
		{
			var freePositions = new List<Vector2Int>();
			
			for(var x = 0; x < _mergeConfig.BoardSize.x; x++)
			for ( var y = 0; y < _mergeConfig.BoardSize.y; y++ )
			{
				if(_itemsMatrix[x,y] == null)
					freePositions.Add( new Vector2Int( x, y ) );
			}

			return freePositions.Random();
		}

		private bool HasFreePosition()
		{
			for ( var x = 0; x < _mergeConfig.BoardSize.x; x++ )
			for ( var y = 0; y < _mergeConfig.BoardSize.y; y++ )
			{
				if ( _itemsMatrix[x, y] == null )
					return true;
			}

			return false;
		}

		private void DisableCellsHints()
		{
			_view.DisableCellsHints();
		}

		private int MatrixIndexToCellIndex( Vector2Int matrixPos, int boardWidth )
		{
			var cellIndex = matrixPos.y * boardWidth + matrixPos.x;
			return cellIndex;
		}

		private Vector2Int CellIndexToMatrixIndex( int cellIndex, int boardWidth )
		{
			var x = cellIndex % boardWidth;
			var y = cellIndex / boardWidth;
			return new Vector2Int( x, y );
		}
	}
}