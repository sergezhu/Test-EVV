namespace Code.MergeSystem
{
	using System;
	using System.Collections.Generic;
	using Code.Database;
	using Sirenix.OdinInspector;
	using UnityEngine;

	[Serializable]
	[CreateAssetMenu(fileName = "MergeConfig", menuName = "Configs/MergeConfig")]
	public class MergeConfig : SerializedScriptableObject
	{
		[SerializeField] private ItemsLibrary itemsLibrary;
		[SerializeField] private float raycastDistance = 100;
		[SerializeField] private int spawnedItemsCountByStart = 2;
		[SerializeField] private float draggedItemOffsetY = 1;
		[SerializeField] private float touchHeight = 1;
		
		[Space]
		[SerializeField] private float autoSpawnDelay = 1f;
		[SerializeField] private int autoSpawnMergeLevel = 0;
		
		[Header("Board and cells")]
		[SerializeField] private MergeBoardCellView boardCellPrefab;
		[SerializeField] private Vector2Int boardSize;
		[SerializeField] private Vector2 cellSize;
		[SerializeField] private Vector2 cellSpace;

		[Header("Layers")]
		[SerializeField] private int mergeItemLayer;
		[SerializeField] private int mergeBoardLayer;
		[SerializeField] private int mergeItemReceiverLayer;
		[SerializeField] private int draggedMergeItemLayer;
		
		[Header("Debug")]
		[SerializeField] private int startMergeLevel;

		[Header("Merge Sequence")]
		[SerializeField] private List<MergeItemInfo> mergeSequence;


		public Vector2Int BoardSize => boardSize;

		public float RaycastDistance => raycastDistance;
		public float DraggedItemOffsetY => draggedItemOffsetY;
		public int SpawnedItemsCountByStart => spawnedItemsCountByStart;
		
		public int MergeItemLayer => mergeItemLayer;
		public int MergeBoardLayer => mergeBoardLayer;
		public int MergeItemReceiverLayer => mergeItemReceiverLayer;
		public int DraggedMergeItemLayer => draggedMergeItemLayer;
		
		//public int MergeItemLayerMask => 1 << mergeItemLayer;
		//public int MergeBoardLayerMask => 1 << mergeBoardLayer;
		//public int MergeItemReceiverLayerMask => 1 << mergeItemReceiverLayer;
		//public int DraggedMergeItemLayerMask => 1 << draggedMergeItemLayer;
		
		public int StartMergeLevel => startMergeLevel;
		public MergeBoardCellView BoardCellPrefab => boardCellPrefab;
		public Vector2 CellSize => cellSize;
		public Vector2 CellSpace => cellSpace;
		public float TouchHeight => touchHeight;

		public float AutoSpawnDelay => autoSpawnDelay;
		public int AutoSpawnMergeLevel => autoSpawnMergeLevel;

		public int GetMergeLevel(uint itemID)
		{
			int index = mergeSequence.FindIndex(d => d.DbInfo.ID == itemID);
			return index;
		}

		public int GetNextMergeLevel(uint itemID)
		{
			return GetMergeLevel(itemID) + 1;
		}

		public ItemDbInfo GetMergeItem(int mergeLevel)
		{
			return mergeSequence[mergeLevel].DbInfo;
		}

		public IDatabaseItem GetMergeDbItem(int mergeLevel)
		{
			ItemDbInfo item = GetMergeItem(mergeLevel);
			IDatabaseItem dbItem = itemsLibrary.GetDbItem(item.ID);

			return dbItem;
		}
		
		public bool HasNextMergeLevel(uint itemId)
		{
			int mergeLevel = GetMergeLevel(itemId);
			return mergeLevel != mergeSequence.Count - 1;
		}

		public bool HasNextMergeLevel(int currentMergeLevel)
		{
			return currentMergeLevel != mergeSequence.Count - 1;
		}

		public ItemDbInfo GetNextMergeItem(uint itemId)
		{
			int index = mergeSequence.FindIndex(d => d.DbInfo.ID == itemId);
			return mergeSequence[index + 1].DbInfo;
		}

		private void OnValidate()
		{
			#if UNITY_EDITOR

			ValidateMergeSequence();

			#endif
		}

		private void ValidateMergeSequence()
		{
			for (int i = 0; i < mergeSequence.Count; i++)
			{
				MergeItemInfo itemInfo = mergeSequence[i];
				ItemDbInfo dbInfo = itemInfo.DbInfo;
				IDatabaseItem dbItem = itemsLibrary.TryGetItemByName(dbInfo.Name);
				
				if(dbItem is null)
					continue;
				
				uint id = dbItem.ID;
				
				itemInfo.DbInfo = new ItemDbInfo(id, dbInfo.Name);
				mergeSequence[i] = itemInfo;
			}
		}
	}
}