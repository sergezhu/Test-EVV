namespace Code.MergeSystem
{
	using Code.Core;
	using Code.Database;
	using UnityEngine;

	public class MergeItemFactory
	{
		private readonly MergeConfig _mergeConfig;
		private readonly ItemsLibrary _itemsLibrary;
		private readonly IInstantiator _instantiator;

		public MergeItemFactory(MergeConfig mergeConfig, ItemsLibrary itemsLibrary, IInstantiator instantiator)
		{
			_mergeConfig = mergeConfig;
			_itemsLibrary = itemsLibrary;
			_instantiator = instantiator;
		}

		public MergeItem Create(ItemDbInfo itemDbInfo)
		{
			var dbItem = _itemsLibrary.GetItem( itemDbInfo.ID );
			var mergeLevel = _mergeConfig.GetMergeLevel( itemDbInfo.ID );
			var view = CreateView( itemDbInfo );
			var mergeItem = new MergeItem( dbItem, view, mergeLevel );

			return mergeItem;
		}

		public MergeItem Create( int mergeLevel, Vector3 worldPos, Transform viewParent )
		{
			var item = _mergeConfig.GetMergeItem( mergeLevel );
			var dbItem = _itemsLibrary.GetItem( item.ID );
			var view = CreateView( item );
			view.SetInitialGeometry( worldPos, viewParent );
			var mergeItem = new MergeItem( dbItem, view, mergeLevel );

			return mergeItem;
		}

		private MergeItemView CreateView( ItemDbInfo itemDbInfo )
		{
			var info = _itemsLibrary.GetFactoryInfo( itemDbInfo.ID );
			var prefab = info.ItemPrefab;
			var viewObj = _instantiator.Instantiate( prefab );
			var view = viewObj.GetComponent<MergeItemView>();

			return view;
		}
	}
}