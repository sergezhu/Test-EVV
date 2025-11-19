namespace Code.MergeSystem
{
	using System;
	using System.Collections.Generic;
	using Code.Database;
	using Sirenix.OdinInspector;
	using UnityEngine;

	[Serializable]
	[CreateAssetMenu( fileName = "MergeConfig", menuName = "Configs/MergeConfig")]
	public class MergeConfig : SerializedScriptableObject
	{
		[SerializeField] private ItemsLibrary _itemsLibrary;
		[SerializeField] private Vector2Int _boardSize;
		[SerializeField] private float _raycastDistance = 100;
		[SerializeField] private int _spawnedItemsCountByStart = 1;
		[SerializeField] private float _draggedItemOffsetY = 1;
		
		[Header("Layers")]
		[SerializeField] private LayerMask _mergeItemLayer;
		[SerializeField] private LayerMask _mergeBoardLayer;
		[SerializeField] private LayerMask _mergeItemReceiverLayer;
		[SerializeField] private LayerMask _draggedMergeItemLayer;

		[Header( "Merge Sequence" )]
		[SerializeField] private List<MergeItemInfo> _mergeSequence;


		public Vector2Int BoardSize => _boardSize;

		public float RaycastDistance => _raycastDistance;
		public float DraggedItemOffsetY => _draggedItemOffsetY;
		public int SpawnedItemsCountByStart => _spawnedItemsCountByStart;
		public int MergeItemLayer => _mergeItemLayer.value;
		public int MergeBoardLayer => _mergeBoardLayer.value;
		public int MergeItemReceiverLayer => _mergeItemReceiverLayer.value;
		public int DraggedMergeItemLayer => _draggedMergeItemLayer.value;

		public int GetMergeLevel( uint itemID )
		{
			var index = _mergeSequence.FindIndex( d => d.DbInfo.ID == itemID );
			return index;
		}

		public int GetNextMergeLevel( uint itemID )
		{
			return GetMergeLevel( itemID ) + 1;
		}

		public ItemDbInfo GetMergeItem( int mergeLevel )
		{
			return _mergeSequence[mergeLevel].DbInfo;
		}

		public IDatabaseItem GetMergeDbItem( int mergeLevel )
		{
			var item = GetMergeItem( mergeLevel );
			var dbItem = _itemsLibrary.GetDbItem( item.ID );
			
			return dbItem;
		}

		public int GetBuyCost( int mergeLevel )
		{
			return _mergeSequence[mergeLevel].BuyCost;
		}

		public float GetRewardPerHit( int mergeLevel )
		{
			return _mergeSequence[mergeLevel].RewardPerHit;
		}

		public int GetBoughtsCountToUpgrade( int mergeLevel )
		{
			return _mergeSequence[mergeLevel].BoughtsCountToUpgrade;
		}

		public bool HasNextMergeLevel( uint itemId )
		{
			var mergeLevel = GetMergeLevel( itemId );
			return mergeLevel != _mergeSequence.Count - 1;
		}

		public bool HasNextMergeLevel( int currentMergeLevel )
		{
			return currentMergeLevel != _mergeSequence.Count - 1;
		}

		public ItemDbInfo GetNextMergeItem( uint itemId )
		{
			var index = _mergeSequence.FindIndex(d => d.DbInfo.ID == itemId );
			return _mergeSequence[index + 1].DbInfo;
		}
	}
}

