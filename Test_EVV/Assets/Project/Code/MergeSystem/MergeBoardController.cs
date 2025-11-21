namespace Code.MergeSystem
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Code.Core;
	using Code.Database;
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
		private readonly MergeItemFactory itemFactory;

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

		public MergeBoardController(MergeBoardView view, MergeItemFactory itemFactory, MergeConfig mergeConfig,
									ITouchInput touchInput, BoardState boardState)
		{
			this.view = view;
			this.mergeConfig = mergeConfig;
			this.touchInput = touchInput;
			this.boardState = boardState;
			this.itemFactory = itemFactory;

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
		public ReactiveCommand<int> IsItemRollback { get; } = new ReactiveCommand<int>();
		public ReactiveCommand<int> IsItemSpawned { get; } = new ReactiveCommand<int>();
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
		
		public void SpawnRandomItem()
		{
			int mergeLevel = boardState.CurrentMergeLevel;
			CreateItemAtRandomPosition(mergeLevel);
			CheckIfItemCreatedFirst(mergeLevel);
		}


		public void BuyItem()
		{
			int mergeLevel = boardState.CurrentMergeLevel;
			CreateItemAtRandomPosition(mergeLevel);

			boardState.AddBuyedToStats(mergeLevel);

			CheckIfItemCreatedFirst(mergeLevel);
		}

		public void TryUpgradeAllItemsToCurrentLevel()
		{
			for (int x = 0; x < mergeConfig.BoardSize.x; x++)
			for (int y = 0; y < mergeConfig.BoardSize.y; y++)
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
			List<MergeCellInfo> result = new List<MergeCellInfo>();

			for (int x = 0; x < mergeConfig.BoardSize.x; x++)
			for (int y = 0; y < mergeConfig.BoardSize.y; y++)
			{
				if (itemsMatrix[x, y] == null)
					continue;

				int level = itemsMatrix[x, y].MergeLevel;
				int cellIndex = MatrixIndexToCellIndex(new Vector2Int(x, y), mergeConfig.BoardSize.x);
				Vector3 cellPos = view.GetSpawnPosition(cellIndex);

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
			List<MergeCellInfo> cellsInfos = GetCellsWithItemsInfo();

			from = default;
			to = default;

			foreach (MergeCellInfo info in cellsInfos)
			{
				int sameLevelItemsCount = cellsInfos.Count(i => i.MergeLevel == info.MergeLevel);

				if (sameLevelItemsCount >= 2)
				{
					MergeCellInfo[] infos = cellsInfos.Where(i => i.MergeLevel == info.MergeLevel).ToArray();
					from = infos[0];
					to = infos[1];
					return true;
				}
			}

			return false;
		}

		public bool TryGetFirstCellWithReceiverItemLevelForMerge(MergeItemReceiver receiver, out MergeCellInfo operationCell)
		{
			operationCell = default;

			if (receiver.HasItem == false)
				return false;

			List<MergeCellInfo> cellsInfos = GetCellsWithItemsInfo();
			int fromIndex = cellsInfos.FindIndex(info => info.MergeLevel == receiver.GetCurrentMergeLevel());

			if (fromIndex == -1)
				return false;

			operationCell = cellsInfos[fromIndex];

			return true;
		}

		public bool TryGetFirstCellForPutToReceiver(MergeItemReceiver receiver, out MergeCellInfo operationCell)
		{
			operationCell = default;
			List<MergeCellInfo> cellsInfos = GetCellsWithItemsInfo();

			if (receiver.HasItem == false)
			{
				bool hasItems = cellsInfos.Count > 0;

				if (hasItems == false)
					return false;

				operationCell = cellsInfos[0];
				return true;
			}

			int fromIndex = cellsInfos.FindIndex(info => info.MergeLevel != receiver.GetCurrentMergeLevel());

			if (fromIndex == -1)
				return false;

			operationCell = cellsInfos[fromIndex];

			return true;
		}

		private void PostInitialize()
		{
			view.CreateCells();
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
			// Here we should subscribe to all items events if need.
			// Use disposable object 'itemsDisposable' for it.
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
				.ThrottleFirst(TimeSpan.FromSeconds(0.02f))
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

			MergeItemView itemViewHit = ThrowRaycastAndGetNearestItem();

			if (itemViewHit != null)
				itemViewHit.OnTouchStartHit();

			Debug.Log($"[OnTouchStart] : {data.WorldDirectionFromMainCamera}, is item hit : {itemViewHit != null}");
			Debug.DrawLine(data.MainCameraPosition, data.MainCameraPosition + data.WorldDirectionFromMainCamera.normalized * mergeConfig.RaycastDistance,
						   Color.red, 2f);

			MergeItem item = FindItemWithStartHit();

			FindCellOrReceiver(out MergeBoardCellView cellView, out MergeItemReceiver receiver);
			bool isCellHit = cellView != null;
			string cellName = isCellHit ? cellView.Name : "none";
			Debug.Log($"[OnTouchStart] : cell : {cellName}");
			
			//Vector2Int? cellIndex = FindItemCoords(item);
			//draggedItem = itemFactory.Create(item);
			
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
			Debug.DrawLine(data.MainCameraPosition, data.MainCameraPosition + data.WorldDirectionFromMainCamera.normalized * mergeConfig.RaycastDistance,
						   Color.red, 2f);

			//var receiver = ThrowRaycastAndGetItemReceiver();
			FindCellOrReceiver(out MergeBoardCellView cellView, out MergeItemReceiver receiver);

			Debug.Log($"[OnTouchEnd] : {data.WorldDirectionFromMainCamera}, receiver is null : {receiver == null}, cellView is null : {cellView == null}");

			if (receiver != null && draggedItem != null)
			{
				if (receiver.CanSwapItem(draggedItem.DbInfo) && 
					allowedOperations.Contains(MergeOperation.EquipWithoutMerge))
				{
					ItemDbInfo receiverItem = receiver.CurrentItem.Value;
					int receiverItemLevel = mergeConfig.GetMergeLevel(receiverItem.ID);
					
					Vector2Int? draggedItemCoord = FindItemCoords(draggedItem);

					if (draggedItemCoord == null)
					{
						throw new InvalidOperationException();
					}

					receiver.ReceiveItem(draggedItem.DbInfo, new MergeItemReceiveOptions() { CanPlayReceiveFX = true });
					DestroyItem(draggedItem);

					if (receiverItemLevel != -1)
					{
						CreateItemAtPosition(draggedItemCoord.Value, receiverItemLevel);
					}

					SaveRestoreData();

					IsItemEquippedWithoutMerge.Execute(draggedItem.MergeLevel);

					draggedItem = null;

					return;
				}

				if (receiver.CanMergeItem(draggedItem.DbInfo) &&
					mergeConfig.HasNextMergeLevel(draggedItem.DbInfo.ID) &&
					allowedOperations.Contains(MergeOperation.EquipWithMerge))
				{
					int nextLevel = mergeConfig.GetNextMergeLevel(draggedItem.DbInfo.ID);
					ItemDbInfo nextItem = mergeConfig.GetMergeItem(nextLevel);
					
					receiver.ReceiveItem(nextItem, new MergeItemReceiveOptions { CanPlayReceiveFX = true });
					
					DestroyItem(draggedItem);
					SaveRestoreData();

					IsItemEquippedWithMerge.Execute(nextLevel);

					draggedItem = null;

					boardState.AddMergedToStats(nextLevel);

					CheckIfItemCreatedFirst(nextLevel);
					return;
				}
			}

			if (cellView != null)
			{
				bool canMergeToCell = CanMergeToCell(cellView, out int cellIndex, out Vector2Int matrixPos);

				if (canMergeToCell && allowedOperations.Contains(MergeOperation.MergeOnBoard))
				{
					MergeItem item = itemsMatrix[matrixPos.x, matrixPos.y];
					
					DoMerge(draggedItem, item);
					
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
			draggedItem.SetPosition(data.WorldProjectionFromMainCamera + view.transform.up * mergeConfig.DraggedItemOffsetY);

			UpdateCellsHints();
		}

		private void UpdateOtherCellsCoordsCanBeMerged()
		{
			int mergeLevel = draggedItem.MergeLevel;

			for (int x = 0; x < mergeConfig.BoardSize.x; x++)
			{
				for (int y = 0; y < mergeConfig.BoardSize.y; y++)
				{
					MergeItem item = itemsMatrix[x, y];

					if (item == null)
						continue;

					if (item != draggedItem && item.MergeLevel == mergeLevel)
					{
						otherCanBeMergedItemsCoords.Add(new Vector2Int(x, y));
					}
				}
			}
		}

		private void DoMerge(MergeItem fromItem, MergeItem toItem)
		{
			Debug.Log("MERGE");

			IsMergeProcessing = true;

			int nextMergeLevel = toItem.MergeLevel + 1;
			
			Vector2Int? toItemCoord = FindItemCoords(toItem);

			if (toItemCoord == null)
			{
				throw new InvalidOperationException("Target item not found");
			}

			fromItem.PlayHideAnimation(() =>
			{
				DestroyItem(fromItem);
			});

			toItem.PlayHideAnimation(() =>
			{
				DestroyItem(toItem);
				CreateItemAt(toItemCoord.Value, nextMergeLevel);
				ResubscribeMergeItems();

				boardState.AddMergedToStats(nextMergeLevel);

				MergeItem item = itemsMatrix[toItemCoord.Value.x, toItemCoord.Value.y];
				
				item.PlayMergeFX();
				item.PlayShowAnimation(null);

				CheckIfItemCreatedFirst(nextMergeLevel);
				SaveRestoreData();

				IsMergeProcessing = false;

				IsItemsMergedOnBoard.Execute(nextMergeLevel);
			});
		}

		private void TryRollbackItem(MergeItem fromItem)
		{
			if (fromItem != null)
			{
				fromItem.SetDefaultItemLayer();
				fromItem.Rollback();
				
				IsItemRollback.Execute(fromItem.MergeLevel);
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
			bool isItemsValid = fromItem != null && 
								toItem != null && 
								fromItem != toItem && 
								fromItem.MergeLevel == toItem.MergeLevel;

			if (isItemsValid == false)
				return false;

			bool hasNextMergeLevel = mergeConfig.HasNextMergeLevel(toItem.DbInfo.ID);

			return hasNextMergeLevel;
		}

		/// <summary>
		/// Checks if item with merge level created first
		/// </summary>
		/// <param name="mergeLevel"></param>
		private void CheckIfItemCreatedFirst(int mergeLevel)
		{
			MergeLevelStatistic? currentStats = boardState.MergeStatistic.GetStatistic(mergeLevel);

			if (currentStats != null && currentStats.Value.CreatedCount == 1)
			{
				IsMergeItemCreatedFirst.Execute(mergeLevel);
			}
		}

		private void TryUpgradeItemToCurrentLevel(Vector2Int coord)
		{
			MergeItem fromItem = itemsMatrix[coord.x, coord.y];
			int fromItemLevel = mergeConfig.GetMergeLevel(fromItem.DbInfo.ID);
			int targetMergeLevel = boardState.CurrentMergeLevel;
			bool canUpgrade = fromItemLevel < targetMergeLevel;

			if (canUpgrade)
			{
				fromItem.PlayHideAnimation(() =>
				{
					DestroyItem(fromItem);
					CreateItemAt(coord, targetMergeLevel);
					ResubscribeMergeItems();

					MergeItem item = itemsMatrix[coord.x, coord.y];
					item.PlayMergeFX();
					item.PlayShowAnimation(null);

					boardState.AddUpgradeCountToStats();
				});
			}
		}

		private void DestroyItem(MergeItem item)
		{
			Vector2Int? firstItemCoord = FindItemCoords(item);

			if (firstItemCoord == null)
			{
				throw new InvalidOperationException($"Not found item {item.DbInfo.Name}");
			}
			
			DestroyItem(firstItemCoord.Value);
		}

		private void DestroyItem(Vector2Int coord)
		{
			MergeItem item = itemsMatrix[coord.x, coord.y];
			
			Debug.Log($"DestroyItem : {coord}, merge level {item.MergeLevel + 1}");
			
			item.DestroyView();
			item.Dispose();
			
			itemsMatrix[coord.x, coord.y] = null;
		}

		private void UpdateCellsHints()
		{
			FindCellOrReceiver(out MergeBoardCellView cellView, out MergeItemReceiver receiver);

			if (currentCellView == cellView || (currentCellView == null && cellView == null))
				return;

			bool canMergeCurrent = CanMergeToCell(currentCellView, out int currentCellIndex, out Vector2Int currentCellCoord);
			bool isCurrentOtherCanBeMerged = otherCanBeMergedItemsCoords.Contains(currentCellCoord);
			CellInteractionState newCurrentState = isCurrentOtherCanBeMerged ? CellInteractionState.OtherMergeable : CellInteractionState.Default;
			
			view.SetCellState(currentCellView, newCurrentState);

			bool canMergeNext = CanMergeToCell(cellView, out int nextCellIndex, out Vector2Int nextCellCoord);
			CellInteractionState newNextState = canMergeNext ? CellInteractionState.Success : CellInteractionState.Fail;
			
			view.SetCellState(cellView, newNextState);

			Debug.Log($"UpdateCellsHints : UpdateCellsHints, canMerge {canMergeNext}, isCurrentOther {isCurrentOtherCanBeMerged}, curState : {newCurrentState}, nextState : {newNextState}");

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

			for (int i = 0; i < otherCanBeMergedItemsCoords.Count; i++)
			{
				Vector2Int coord = otherCanBeMergedItemsCoords[i];
				bool isOther = coord != matrixIndex;

				if (isOther == false)
					continue;

				int otherCellIndex = MatrixIndexToCellIndex(coord, mergeConfig.BoardSize.x);
				
				view.SetCellState(otherCellIndex, CellInteractionState.OtherMergeable);
			}

			MergeItem attachedItem = itemsMatrix[matrixIndex.x, matrixIndex.y];
			bool hasAttachedItem = attachedItem != null;
			bool canMerge = false;

			if (hasAttachedItem)
			{
				canMerge = HasSameLevelAndExistNextLevel(draggedItem, attachedItem);
			}

			return canMerge;
		}

		private void FindCellOrReceiver(out MergeBoardCellView cellView, out MergeItemReceiver receiver)
		{
			receiver = ThrowRaycastAndGetItemReceiver();
			cellView = ThrowRaycastAndGetNearestCell();
			
			// Priority is for CellView

			if (cellView == null)
			{
				if (receiver != null)
				{
					currentCellView = null;

					if (receiverUnderPointer == null)
					{
						receiverUnderPointer = receiver;
						DisableCellsHints();
					}
				}
				else
				{
					receiverUnderPointer = null;
				}
			}
			else
			{
				receiverUnderPointer = null;
			}
		}

		private TComponent ThrowRaycastAndGetNearestObject<TComponent>(Vector3 camPosition, Vector3 dirFromCam, int mask) where TComponent : Component
		{
			Physics.RaycastNonAlloc(camPosition, dirFromCam, hits, mergeConfig.RaycastDistance, mask);

			List<TComponent> components = hits
				.Where(hit => hit.collider != null)
				.Select(hit => hit.collider.gameObject.GetComponent<TComponent>())
				.Where(comp => comp != null)
				.ToList();

			TComponent firstComponent = null;

			if (components.Count > 0)
			{
				firstComponent = components[0];
			}

			return firstComponent;
		}

		private MergeItemView ThrowRaycastAndGetNearestItem()
		{
			int mask = 1 << mergeConfig.MergeItemLayer;
			return ThrowRaycastAndGetNearestObject<MergeItemView>(lastTouchData.MainCameraPosition, lastTouchData.WorldDirectionFromMainCamera, mask);
		}

		private MergeBoardCellView ThrowRaycastAndGetNearestCell()
		{
			int mask = 1 << mergeConfig.MergeBoardLayer;
			return ThrowRaycastAndGetNearestObject<MergeBoardCellView>(lastTouchData.MainCameraPosition, lastTouchData.WorldDirectionFromMainCamera, mask);
		}

		private MergeItemReceiver ThrowRaycastAndGetItemReceiver()
		{
			int mask = 1 << mergeConfig.MergeItemReceiverLayer;
			return ThrowRaycastAndGetNearestObject<MergeItemReceiver>(lastTouchData.MainCameraPosition, lastTouchData.WorldDirectionFromMainCamera, mask);
		}

		private MergeItem FindItemWithStartHit()
		{
			for (int x = 0; x < mergeConfig.BoardSize.x; x++)
			{
				for (int y = 0; y < mergeConfig.BoardSize.y; y++)
				{
					if (itemsMatrix[x, y] == null)
						continue;

					if (itemsMatrix[x, y].TouchStartFlag)
						return itemsMatrix[x, y];
				}
			}

			return null;
		}

		private MergeItem FindItemWithEndHit()
		{
			for (int x = 0; x < mergeConfig.BoardSize.x; x++)
			{
				for (int y = 0; y < mergeConfig.BoardSize.y; y++)
				{
					if (itemsMatrix[x, y] == null)
						continue;

					if (itemsMatrix[x, y].TouchEndFlag)
						return itemsMatrix[x, y];
				}
			}

			return null;
		}

		private Vector2Int? FindItemCoords(MergeItem item)
		{
			if (item == null)
				throw new InvalidOperationException();

			for (int x = 0; x < mergeConfig.BoardSize.x; x++)
			{
				for (int y = 0; y < mergeConfig.BoardSize.y; y++)
				{
					if (itemsMatrix[x, y] == null)
						continue;

					if (itemsMatrix[x, y] == item)
						return new Vector2Int(x, y);
				}
			}

			return null;
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
			IReadOnlyList<MergeBoardCellRecord> restoredItemsData = boardState.MergeBoardRestoreData;
			bool canRestore = restoredItemsData.Count > 0;

			if (canRestore)
			{
				restoredItemsData.ForEach(data => { CreateItemAtPosition(new Vector2Int(data.X, data.Y), data.MergeLevel); });
			}
			else
			{
				for (int i = 0; i < mergeConfig.SpawnedItemsCountByStart; i++)
				{
					CreateItemAtRandomPosition(boardState.CurrentMergeLevel);
				}
			}
		}

		private List<MergeBoardCellRecord> GetRestoredItemsData()
		{
			List<MergeBoardCellRecord> restoredItemsData = new List<MergeBoardCellRecord>();

			for (int x = 0; x < mergeConfig.BoardSize.x; x++)
			{
				for (int y = 0; y < mergeConfig.BoardSize.y; y++)
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
			}

			return restoredItemsData;
		}

		private void CreateItemAtRandomPosition(int mergeLevel)
		{
			Vector2Int matrixPos = GetRandomFreePosition();
			CreateItemWithFX(matrixPos, mergeLevel);
		}

		private void CreateItemAtPosition(Vector2Int matrixPos, int mergeLevel)
		{
			CreateItemWithFX(matrixPos, mergeLevel);
		}

		private void CreateItemWithFX(Vector2Int matrixPos, int mergeLevel)
		{
			CreateItemAt(matrixPos, mergeLevel);

			MergeItem item = itemsMatrix[matrixPos.x, matrixPos.y];
			item.PlaySpawnFX();
			item.PlayShowAnimation(null);
		}

		private void CreateItemAt(Vector2Int matrixPos, int mergeLevel)
		{
			int cellIndex = MatrixIndexToCellIndex(matrixPos, mergeConfig.BoardSize.x);
			Vector3 worldPos = view.GetSpawnPosition(cellIndex);
			Transform itemRoot = view.GetItemRoot(cellIndex);
			itemsMatrix[matrixPos.x, matrixPos.y] = itemFactory.Create(mergeLevel, worldPos, itemRoot);

			IsItemSpawned.Execute(mergeLevel);
			
			Debug.Log($"Create item at {matrixPos}, merge level {mergeLevel + 1}");
		}

		private void DetroyMergeItems()
		{
			for (int x = 0; x < mergeConfig.BoardSize.x; x++)
			{
				for (int y = 0; y < mergeConfig.BoardSize.y; y++)
				{
					if (itemsMatrix[x, y] == null)
						continue;

					DestroyItem(new Vector2Int(x, y));
				}
			}
		}

		private void SaveRestoreData()
		{
			List<MergeBoardCellRecord> restoreData = GetRestoredItemsData();
			boardState.UpdateRestoreData(restoreData);
		}


		private Vector2Int GetRandomFreePosition()
		{
			List<Vector2Int> freePositions = new List<Vector2Int>();

			for (int x = 0; x < mergeConfig.BoardSize.x; x++)
			{
				for (int y = 0; y < mergeConfig.BoardSize.y; y++)
				{
					if (itemsMatrix[x, y] == null)
					{
						freePositions.Add(new Vector2Int(x, y));
					}
				}
			}

			return freePositions.Random();
		}

		private bool HasFreePosition()
		{
			for (int x = 0; x < mergeConfig.BoardSize.x; x++)
			{
				for (int y = 0; y < mergeConfig.BoardSize.y; y++)
				{
					if (itemsMatrix[x, y] == null)
						return true;
				}
			}

			return false;
		}

		private void DisableCellsHints()
		{
			view.DisableCellsHints();
		}

		private int MatrixIndexToCellIndex(Vector2Int matrixPos, int boardWidth)
		{
			int cellIndex = matrixPos.y * boardWidth + matrixPos.x;
			return cellIndex;
		}

		private Vector2Int CellIndexToMatrixIndex(int cellIndex, int boardWidth)
		{
			int x = cellIndex % boardWidth;
			int y = cellIndex / boardWidth;
			return new Vector2Int(x, y);
		}
	}
}