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

		public int GetMergeLevel(uint itemID)
		{
			var index = mergeSequence.FindIndex(d => d.DbInfo.ID == itemID);
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
			var item = GetMergeItem(mergeLevel);
			var dbItem = itemsLibrary.GetDbItem(item.ID);

			return dbItem;
		}
		
		public bool HasNextMergeLevel(uint itemId)
		{
			var mergeLevel = GetMergeLevel(itemId);
			return mergeLevel != mergeSequence.Count - 1;
		}

		public bool HasNextMergeLevel(int currentMergeLevel)
		{
			return currentMergeLevel != mergeSequence.Count - 1;
		}

		public ItemDbInfo GetNextMergeItem(uint itemId)
		{
			var index = mergeSequence.FindIndex(d => d.DbInfo.ID == itemId);
			return mergeSequence[index + 1].DbInfo;
		}
	}
}