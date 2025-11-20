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
		[SerializeField] private Vector2Int boardSize;
		[SerializeField] private float raycastDistance = 100;
		[SerializeField] private int spawnedItemsCountByStart = 1;
		[SerializeField] private float draggedItemOffsetY = 1;

		[Header("Layers")]
		[SerializeField] private LayerMask mergeItemLayer;
		[SerializeField] private LayerMask mergeBoardLayer;
		[SerializeField] private LayerMask mergeItemReceiverLayer;
		[SerializeField] private LayerMask draggedMergeItemLayer;
		
		[Header("Debug")]
		[SerializeField] private int startMergeLevel;

		[Header("Merge Sequence")]
		[SerializeField] private List<MergeItemInfo> mergeSequence;


		public Vector2Int BoardSize => boardSize;

		public float RaycastDistance => raycastDistance;
		public float DraggedItemOffsetY => draggedItemOffsetY;
		public int SpawnedItemsCountByStart => spawnedItemsCountByStart;
		public int MergeItemLayer => mergeItemLayer.value;
		public int MergeBoardLayer => mergeBoardLayer.value;
		public int MergeItemReceiverLayer => mergeItemReceiverLayer.value;
		public int DraggedMergeItemLayer => draggedMergeItemLayer.value;
		public int StartMergeLevel => startMergeLevel;

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