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
		private readonly BoardState boardState;
		private readonly CompositeDisposable disposable;
		private readonly MergeItemFactory factory;

		private readonly RaycastHit[] hits;
		private readonly CompositeDisposable itemsDisposable;

		private readonly MergeItem[,] itemsMatrix;
		private readonly MergeConfig mergeConfig;
		private readonly List<Vector2Int> otherCanBeMergedItemsCoords;
		private readonly CompositeDisposable touchDisposable;
		private readonly ITouchInput touchInput;
		private readonly MergeBoardView view;
		private MergeOperation[] allOperations;
		private List<MergeOperation> allowedOperations;
		private MergeBoardCellView currentCellView;
		private MergeItem draggedItem;
		private TouchData lastTouchData;
		private MergeItemReceiver receiverUnderPointer;

		public MergeBoardController(MergeBoardView view, MergeItemFactory factory, MergeConfig mergeConfig,
									ITouchInput touchInput, BoardState boardState)
		{
			this.view = view;
			this.mergeConfig = mergeConfig;
			this.touchInput = touchInput;
			this.boardState = boardState;
			this.factory = factory;

			hits = new RaycastHit[10];
			itemsMatrix = new MergeItem[this.mergeConfig.BoardSize.x, this.mergeConfig.BoardSize.y];
			otherCanBeMergedItemsCoords = new List<Vector2Int>();

			disposable = new CompositeDisposable();
			touchDisposable = new CompositeDisposable();
			itemsDisposable = new CompositeDisposable();
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
			allOperations = new[] { MergeOperation.EquipWithMerge, MergeOperation.EquipWithoutMerge, MergeOperation.MergeOnBoard };
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
			var mergeLevel = boardState.CurrentMergeLevel;
			CreateItemAtRandomPosition(mergeLevel);

			boardState.AddBuyedToStats(mergeLevel);

			CheckIfItemCreatedFirst(mergeLevel);
		}

		public void TryUpgradeAllItemsToCurrentLevel()
		{
			for (var x = 0; x < mergeConfig.BoardSize.x; x++)
			for (var y = 0; y < mergeConfig.BoardSize.y; y++)
			{
				if (itemsMatrix[x, y] == null)
					continue;

				TryUpgradeItemToCurrentLevel(new Vector2Int(x, y));
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
			AllowOperations(allOperations);
		}

		public void DenyAllOperations()
		{
			if (allowedOperations == null)
				allowedOperations = new List<MergeOperation>();
			else
				allowedOperations.Clear();
		}

		public void AllowOperations(IEnumerable<MergeOperation> operations)
		{
			allowedOperations = new List<MergeOperation>();
			operations.ForEach(op => allowedOperations.Add(op));
		}

		public List<MergeCellInfo> GetCellsWithItemsInfo()
		{
			var result = new List<MergeCellInfo>();

			for (var x = 0; x < mergeConfig.BoardSize.x; x++)
			for (var y = 0; y < mergeConfig.BoardSize.y; y++)
			{
				if (itemsMatrix[x, y] == null)
					continue;

				var level = itemsMatrix[x, y].MergeLevel;
				var cellIndex = MatrixIndexToCellIndex(new Vector2Int(x, y), mergeConfig.BoardSize.x);
				var cellPos = view.GetSpawnPosition(cellIndex);

				result.Add(new MergeCellInfo
				{
					CellIndex = cellIndex,
					Coord = new Vector2Int(x, y),
					MergeLevel = level,
					WorldPosition = cellPos
				});
			}

			return result;
		}

		public bool TryGetFirstMergePair(out MergeCellInfo from, out MergeCellInfo to)
		{
			var cellsInfos = GetCellsWithItemsInfo();

			from = default;
			to = default;

			foreach (var info in cellsInfos)
			{
				var sameLevelItemsCount = cellsInfos.Count(i => i.MergeLevel == info.MergeLevel);

				if (sameLevelItemsCount >= 2)
				{
					var infos = cellsInfos.Where(i => i.MergeLevel == info.MergeLevel).ToArray();
					from = infos[0];
					to = infos[1];
					return true;
				}
			}

			return false;
		}

		public bool TryGetFirstRecordForEquipWithMerge(MergeItemReceiver receiver, out MergeCellInfo from)
		{
			from = default;

			if (receiver.HasItem == false)
				return false;

			var cellsInfos = GetCellsWithItemsInfo();
			var fromIndex = cellsInfos.FindIndex(info => info.MergeLevel == receiver.GetCurrentMergeLevel());

			if (fromIndex == -1)
				return false;

			from = cellsInfos[fromIndex];

			return true;
		}

		public bool TryGetFirstRecordForEquipWithoutMerge(MergeItemReceiver receiver, out MergeCellInfo from)
		{
			from = default;
			var cellsInfos = GetCellsWithItemsInfo();

			if (receiver.HasItem == false)
			{
				var hasItems = cellsInfos.Count > 0;

				if (hasItems == false)
					return false;

				from = cellsInfos[0];
				return true;
			}

			var fromIndex = cellsInfos.FindIndex(info => info.MergeLevel != receiver.GetCurrentMergeLevel());

			if (fromIndex == -1)
				return false;

			from = cellsInfos[fromIndex];

			return true;
		}

		private void PostInitialize()
		{
			view.Show();

			AllowAllOperations();
			CreateStartMergeItems();
			SubscribeMergeItems();
			SubscribeTouch();
		}

		private void CleanUp()
		{
			if (draggedItem != null)
			{
				draggedItem.SetDefaultItemLayer();
				draggedItem.Rollback();
				draggedItem = null;
			}

			SaveRestoreData();

			view.Hide();

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
			for (var x = 0; x < mergeConfig.BoardSize.x; x++)
			for (var y = 0; y < mergeConfig.BoardSize.y; y++)
			{
				if (itemsMatrix[x, y] == null)
					continue;

				var item = itemsMatrix[x, y];

				//item.***.Subscribe
			}
		}

		private void SubscribeTouch()
		{
			touchInput.OnTouchStart
				.Where(_ => IsLocked == false)
				.Subscribe(pos => OnTouchStart(pos))
				.AddTo(touchDisposable);

			touchInput.OnTouchEnd
				.Where(_ => IsLocked == false)
				.Subscribe(pos => OnTouchEnd(pos))
				.AddTo(touchDisposable);

			touchInput.OnTouchPositionChanged
				.Where(_ => IsLocked == false)
				.Subscribe(pos => OnTouchPositionChanged(pos))
				.AddTo(touchDisposable);
		}

		private void ClearDragging()
		{
			if (draggedItem != null)
			{
				draggedItem.SetDefaultItemLayer();
				draggedItem.Rollback();
			}

			CrearDraggingVariables();
		}

		private void OnTouchStart(TouchData data)
		{
			if (IsMergeProcessing)
				return;

			lastTouchData = data;

			var itemViewHit = ThrowRaycastAndGetNearestItem();

			if (itemViewHit != null)
				itemViewHit.OnTouchStartHit();

			Debug.Log($"OnTouchStart : {data.WorldDirectionFromMergeCamera}, is hit : {itemViewHit != null}");
			Debug.DrawLine(data.MergeCameraPosition, data.MergeCameraPosition + data.WorldDirectionFromMergeCamera.normalized * mergeConfig.RaycastDistance,
						   Color.red, 2f);

			var item = FindItemWithStartHit();
			draggedItem = item;

			if (draggedItem != null)
			{
				draggedItem.SetDraggedItemLayer();
				UpdateOtherCellsCoordsCanBeMerged();
				UpdateCellsHints();
			}
		}

		private void OnTouchEnd(TouchData data)
		{
			Debug.DrawLine(data.MergeCameraPosition, data.MergeCameraPosition + data.WorldDirectionFromMergeCamera.normalized * mergeConfig.RaycastDistance,
						   Color.red, 2f);

			//var receiver = ThrowRaycastAndGetItemReceiver();
			FindCellOrReceiver(out var cellView, out var receiver);

			Debug.Log($"OnTouchEnd : {data.WorldDirectionFromMergeCamera}, receiver is null : {receiver == null}, cellView is null : {cellView == null}");

			if (receiver != null && draggedItem != null)
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
				if (receiver.CanMergeItem(draggedItem.DbInfo) &&
					mergeConfig.HasNextMergeLevel(draggedItem.DbInfo.ID) &&
					allowedOperations.Contains(MergeOperation.EquipWithMerge))
				{
					var nextLevel = mergeConfig.GetNextMergeLevel(draggedItem.DbInfo.ID);
					var nextItem = mergeConfig.GetMergeItem(nextLevel);
					receiver.ReceiveItem(nextItem, new MergeItemReceiveOptions { CanPlayReceiveFX = true });
					DestroyItem(draggedItem);

					//Debug.Log( "-- set _draggedItem as null in CanMergeItem checking" );
					SaveRestoreData();

					IsItemEquippedWithMerge.Execute(nextLevel);

					draggedItem = null;

					boardState.AddMergedToStats(nextLevel);

					CheckIfItemCreatedFirst(nextLevel);
					return;
				}

			/*var itemViewHit = ThrowRaycastAndGetNearestItem();

			if ( itemViewHit != null )
				itemViewHit.OnTouchEndHit(); */

			if (cellView != null)
			{
				var canMergeToCell = CanMergeToCell(cellView, out var cellIndex, out var matrixPos);

				if (canMergeToCell && allowedOperations.Contains(MergeOperation.MergeOnBoard))
				{
					var item = itemsMatrix[matrixPos.x, matrixPos.y];
					DoMerge(draggedItem, item);
					DestroyItem(draggedItem);
					draggedItem = null;
				}
				else
				{
					TryRollbackItem(draggedItem);
				}
			}
			else
			{
				TryRollbackItem(draggedItem);
			}


			//var item = FindItemWithEndHit();
			//DoMergeOrRollback( _draggedItem, item );

			DisableCellsHints();
			CrearDraggingVariables();
		}

		private void OnTouchPositionChanged(TouchData data)
		{
			if (IsMergeProcessing)
				return;

			lastTouchData = data;

			if (draggedItem == null)
				return;

			//Debug.DrawLine( data.WorldProjectionFromMergeCamera, data.WorldProjectionFromMergeCamera + _view.transform.up * _mergeConfig.DraggedItemOffsetY, Color.red, 0.5f );
			draggedItem.SetPosition(data.WorldProjectionFromMergeCamera + view.transform.up * mergeConfig.DraggedItemOffsetY);

			UpdateCellsHints();
		}

		private void UpdateOtherCellsCoordsCanBeMerged()
		{
			var mergeLevel = draggedItem.MergeLevel;

			for (var x = 0; x < mergeConfig.BoardSize.x; x++)
			for (var y = 0; y < mergeConfig.BoardSize.y; y++)
			{
				var mergeItem = itemsMatrix[x, y];

				if (mergeItem == null)
					continue;

				if (mergeItem != draggedItem && mergeItem.MergeLevel == mergeLevel) otherCanBeMergedItemsCoords.Add(new Vector2Int(x, y));
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

		private void DoMerge(MergeItem fromItem, MergeItem toItem)
		{
			Debug.Log("MERGE");

			IsMergeProcessing = true;

			var nextMergeLevel = toItem.MergeLevel + 1;
			var toItemCoord = FindItemCoords(toItem);

			fromItem.PlayHideAnimation(() => { DestroyItem(fromItem); });

			toItem.PlayHideAnimation(() =>
			{
				DestroyItem(toItem);
				CreateItemAt(toItemCoord, nextMergeLevel);
				ResubscribeMergeItems();

				boardState.AddMergedToStats(nextMergeLevel);

				var item = itemsMatrix[toItemCoord.x, toItemCoord.y];
				item.PlayMergeFX();
				item.PlayShowAnimation(null);

				CheckIfItemCreatedFirst(nextMergeLevel);

				IsMergeProcessing = false;

				SaveRestoreData();
				IsItemsMergedOnBoard.Execute(nextMergeLevel);
			});
		}

		private void TryRollbackItem(MergeItem fromItem)
		{
			if (fromItem != null)
			{
				fromItem.SetDefaultItemLayer();
				fromItem.Rollback();
			}
		}

		private void CrearDraggingVariables()
		{
			draggedItem = null;
			currentCellView = null;
			otherCanBeMergedItemsCoords.Clear();
		}

		private bool HasSameLevelAndExistNextLevel(MergeItem fromItem, MergeItem toItem)
		{
			var isItemsValid = fromItem != null && toItem != null && fromItem != toItem && fromItem.MergeLevel == toItem.MergeLevel;

			if (isItemsValid == false)
				return false;

			var hasNextMergeLevel = mergeConfig.HasNextMergeLevel(toItem.DbInfo.ID);

			return hasNextMergeLevel;
		}

		private void CheckIfItemCreatedFirst(int mergeLevel)
		{
			var currentStats = boardState.MergeStatistic.GetStatistic(mergeLevel);
			if (currentStats != null && currentStats.Value.CreatedCount == 1) IsMergeItemCreatedFirst.Execute(mergeLevel);
		}

		private void TryUpgradeItemToCurrentLevel(Vector2Int coord)
		{
			var fromItem = itemsMatrix[coord.x, coord.y];
			var fromItemLevel = mergeConfig.GetMergeLevel(fromItem.DbInfo.ID);
			var targetMergeLevel = boardState.CurrentMergeLevel;
			var canUpgrade = fromItemLevel < targetMergeLevel;

			if (canUpgrade)
				fromItem.PlayHideAnimation(() =>
				{
					DestroyItem(fromItem);
					CreateItemAt(coord, targetMergeLevel);
					ResubscribeMergeItems();

					var item = itemsMatrix[coord.x, coord.y];
					item.PlayMergeFX();
					item.PlayShowAnimation(null);

					boardState.AddUpgradeCountToStats();
				});
		}

		private void DestroyItem(MergeItem item)
		{
			var firstItemCoord = FindItemCoords(item);
			DestroyItem(firstItemCoord);
		}

		private void DestroyItem(Vector2Int coord)
		{
			itemsMatrix[coord.x, coord.y].DestroyView();
			itemsMatrix[coord.x, coord.y].Dispose();
			itemsMatrix[coord.x, coord.y] = null;
		}

		private void UpdateCellsHints()
		{
			FindCellOrReceiver(out var cellView, out var receiver);

			if (currentCellView == cellView || (currentCellView == null && cellView == null))
				return;

			//Debug.Log( "UpdateCellsHints : UpdateCellsHints" );

			var canMergeCurrent = CanMergeToCell(currentCellView, out var currentCellIndex, out var currentCellCoord);
			//var currentCellIndex = _view.GetCellIndex( _currentCellView );
			//var currentCellCoord = CellIndexToMatrixIndex( currentCellIndex, _mergeConfig.BoardSize.x );
			var isCurrentOtherCanBeMerged = otherCanBeMergedItemsCoords.Contains(currentCellCoord);
			var newCurrentState = isCurrentOtherCanBeMerged ? CellInteractionState.OtherMergeable : CellInteractionState.Default;
			view.SwitchToState(currentCellView, newCurrentState);


			var canMergeNext = CanMergeToCell(cellView, out var nextCellIndex, out var nextCellCoord);
			var newNextState = canMergeNext ? CellInteractionState.Success : CellInteractionState.Fail;
			view.SwitchToState(cellView, newNextState);


			Debug.Log(
				$"UpdateCellsHints : UpdateCellsHints, canMerge {canMergeNext}, isCurrentOther {isCurrentOtherCanBeMerged}, curState : {newCurrentState}, nextState : {newNextState}");

			/*if( canMerge )
			{
				_view.SwitchToState( cellView, CellInteractionState.Success );
			}
			else
			{
				_view.SwitchToState( cellView, CellInteractionState.Fail );
			}*/

			currentCellView = cellView;
		}

		private bool CanMergeToCell(MergeBoardCellView cellView, out int cellIndex, out Vector2Int matrixIndex)
		{
			if (cellView == null)
			{
				cellIndex = -1;
				matrixIndex = new Vector2Int(-1, -1);
				return false;
			}

			cellIndex = view.GetCellIndex(cellView);
			matrixIndex = CellIndexToMatrixIndex(cellIndex, mergeConfig.BoardSize.x);

			for (var i = 0; i < otherCanBeMergedItemsCoords.Count; i++)
			{
				var coord = otherCanBeMergedItemsCoords[i];
				var isOther = coord != matrixIndex;

				if (isOther == false)
					continue;

				var otherCellIndex = MatrixIndexToCellIndex(coord, mergeConfig.BoardSize.x);
				view.SwitchToState(otherCellIndex, CellInteractionState.OtherMergeable);
			}

			var attachedItem = itemsMatrix[matrixIndex.x, matrixIndex.y];
			var hasAttachedItem = attachedItem != null;
			var canMerge = false;

			if (hasAttachedItem) canMerge = HasSameLevelAndExistNextLevel(draggedItem, attachedItem);

			return canMerge;
		}

		private void FindCellOrReceiver(out MergeBoardCellView cellView, out MergeItemReceiver receiver)
		{
			receiver = ThrowRaycastAndGetItemReceiver();
			cellView = ThrowRaycastAndGetNearestCell();

			if (cellView == null)
			{
				if (receiver != null) currentCellView = null;

				if (receiver != null && receiverUnderPointer == null)
				{
					receiverUnderPointer = receiver;
					DisableCellsHints();
				}

				if (receiver == null && receiverUnderPointer != null) receiverUnderPointer = receiver;
			}
			else
			{
				receiverUnderPointer = null;
			}
		}

		private TComponent ThrowRaycastAndGetNearestObject<TComponent>(Vector3 camPosition, Vector3 dirFromCam, int mask) where TComponent : Component
		{
			Physics.RaycastNonAlloc(camPosition, dirFromCam, hits, mergeConfig.RaycastDistance, mask);

			var components = hits
				.Where(hit => hit.collider != null)
				.Select(hit => hit.collider.gameObject.GetComponent<TComponent>())
				.ToList();

			TComponent firstComponent = null;

			if (components.Count > 0) firstComponent = components[0];

			return firstComponent;
		}

		private MergeItemView ThrowRaycastAndGetNearestItem()
		{
			var mask = 1 << mergeConfig.MergeItemLayer;
			return ThrowRaycastAndGetNearestObject<MergeItemView>(lastTouchData.MergeCameraPosition, lastTouchData.WorldDirectionFromMergeCamera, mask);
		}

		private MergeBoardCellView ThrowRaycastAndGetNearestCell()
		{
			var mask = 1 << mergeConfig.MergeBoardLayer;
			return ThrowRaycastAndGetNearestObject<MergeBoardCellView>(lastTouchData.MergeCameraPosition, lastTouchData.WorldDirectionFromMergeCamera, mask);
		}

		private MergeItemReceiver ThrowRaycastAndGetItemReceiver()
		{
			var mask = 1 << mergeConfig.MergeItemReceiverLayer;
			return ThrowRaycastAndGetNearestObject<MergeItemReceiver>(lastTouchData.MainCameraPosition, lastTouchData.WorldDirectionFromMainCamera, mask);
		}

		private MergeItem FindItemWithStartHit()
		{
			for (var x = 0; x < mergeConfig.BoardSize.x; x++)
			for (var y = 0; y < mergeConfig.BoardSize.y; y++)
			{
				if (itemsMatrix[x, y] == null)
					continue;

				if (itemsMatrix[x, y].TouchStartFlag)
					return itemsMatrix[x, y];
			}

			return null;
		}

		private MergeItem FindItemWithEndHit()
		{
			for (var x = 0; x < mergeConfig.BoardSize.x; x++)
			for (var y = 0; y < mergeConfig.BoardSize.y; y++)
			{
				if (itemsMatrix[x, y] == null)
					continue;

				if (itemsMatrix[x, y].TouchEndFlag)
					return itemsMatrix[x, y];
			}

			return null;
		}

		private Vector2Int FindItemCoords(MergeItem item)
		{
			if (item == null)
				throw new InvalidOperationException();

			for (var x = 0; x < mergeConfig.BoardSize.x; x++)
			for (var y = 0; y < mergeConfig.BoardSize.y; y++)
			{
				if (itemsMatrix[x, y] == null)
					continue;

				if (itemsMatrix[x, y] == item)
					return new Vector2Int(x, y);
			}

			return new Vector2Int(-1, -1);
		}

		private void UnsubscribeTouch()
		{
			touchDisposable.Clear();
		}

		private void UnsubscribeMergeItems()
		{
			itemsDisposable.Clear();
		}

		private void CreateStartMergeItems()
		{
			//var restoredItemsData = GetRestoredItemsData();
			var restoredItemsData = boardState.MergeBoardRestoreData;
			var canRestore = restoredItemsData.Count > 0;

			if (canRestore)
				restoredItemsData.ForEach(data => { CreateItemAtPosition(new Vector2Int(data.X, data.Y), data.MergeLevel); });
			else
				for (var i = 0; i < mergeConfig.SpawnedItemsCountByStart; i++)
					CreateItemAtRandomPosition(boardState.CurrentMergeLevel);
		}

		private List<MergeBoardCellRecord> GetRestoredItemsData()
		{
			var restoredItemsData = new List<MergeBoardCellRecord>();

			for (var x = 0; x < mergeConfig.BoardSize.x; x++)
			for (var y = 0; y < mergeConfig.BoardSize.y; y++)
			{
				if (itemsMatrix[x, y] == null)
					continue;

				restoredItemsData.Add(new MergeBoardCellRecord
				{
					X = x,
					Y = y,
					MergeLevel = itemsMatrix[x, y].MergeLevel
				});
			}

			return restoredItemsData;
		}

		private void CreateItemAtRandomPosition(int mergeLevel)
		{
			var matrixPos = GetRandomFreePosition();
			CreateItemWithFX(matrixPos, mergeLevel);
		}

		private void CreateItemAtPosition(Vector2Int matrixPos, int mergeLevel)
		{
			CreateItemWithFX(matrixPos, mergeLevel);
		}

		private void CreateItemWithFX(Vector2Int matrixPos, int mergeLevel)
		{
			CreateItemAt(matrixPos, mergeLevel);

			var item = itemsMatrix[matrixPos.x, matrixPos.y];
			item.PlaySpawnFX();
			item.PlayShowAnimation(null);
		}

		private void CreateItemAt(Vector2Int matrixPos, int mergeLevel)
		{
			var cellIndex = MatrixIndexToCellIndex(matrixPos, mergeConfig.BoardSize.x);
			var worldPos = view.GetSpawnPosition(cellIndex);
			var itemRoot = view.GetItemRoot(cellIndex);
			itemsMatrix[matrixPos.x, matrixPos.y] = factory.Create(mergeLevel, worldPos, itemRoot);
		}

		private void DetroyMergeItems()
		{
			for (var x = 0; x < mergeConfig.BoardSize.x; x++)
			for (var y = 0; y < mergeConfig.BoardSize.y; y++)
			{
				if (itemsMatrix[x, y] == null)
					continue;

				DestroyItem(new Vector2Int(x, y));
			}
		}

		private void SaveRestoreData()
		{
			var restoreData = GetRestoredItemsData();
			boardState.UpdateRestoreData(restoreData);
		}


		private Vector2Int GetRandomFreePosition()
		{
			var freePositions = new List<Vector2Int>();

			for (var x = 0; x < mergeConfig.BoardSize.x; x++)
			for (var y = 0; y < mergeConfig.BoardSize.y; y++)
				if (itemsMatrix[x, y] == null)
					freePositions.Add(new Vector2Int(x, y));

			return freePositions.Random();
		}

		private bool HasFreePosition()
		{
			for (var x = 0; x < mergeConfig.BoardSize.x; x++)
			for (var y = 0; y < mergeConfig.BoardSize.y; y++)
				if (itemsMatrix[x, y] == null)
					return true;

			return false;
		}

		private void DisableCellsHints()
		{
			view.DisableCellsHints();
		}

		private int MatrixIndexToCellIndex(Vector2Int matrixPos, int boardWidth)
		{
			var cellIndex = matrixPos.y * boardWidth + matrixPos.x;
			return cellIndex;
		}

		private Vector2Int CellIndexToMatrixIndex(int cellIndex, int boardWidth)
		{
			var x = cellIndex % boardWidth;
			var y = cellIndex / boardWidth;
			return new Vector2Int(x, y);
		}
	}
}