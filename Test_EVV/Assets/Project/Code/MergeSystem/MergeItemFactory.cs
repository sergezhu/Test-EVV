namespace Code.MergeSystem
{
	using Code.Core;
	using Code.Database;
	using UnityEngine;

	public class MergeItemFactory
	{
		private readonly IInstantiator instantiator;
		private readonly ItemsLibrary itemsLibrary;
		private readonly MergeConfig mergeConfig;

		public MergeItemFactory(MergeConfig mergeConfig, ItemsLibrary itemsLibrary, IInstantiator instantiator)
		{
			this.mergeConfig = mergeConfig;
			this.itemsLibrary = itemsLibrary;
			this.instantiator = instantiator;
		}

		public MergeItem Create(ItemDbInfo itemDbInfo)
		{
			var dbItem = itemsLibrary.GetItem(itemDbInfo.ID);
			var mergeLevel = mergeConfig.GetMergeLevel(itemDbInfo.ID);
			var view = CreateView(itemDbInfo);
			var mergeItem = new MergeItem(dbItem, view, mergeLevel);

			return mergeItem;
		}

		public MergeItem Create(int mergeLevel, Vector3 worldPos, Transform viewParent)
		{
			var item = mergeConfig.GetMergeItem(mergeLevel);
			var dbItem = itemsLibrary.GetItem(item.ID);
			var view = CreateView(item);
			view.SetInitialGeometry(worldPos, viewParent);
			var mergeItem = new MergeItem(dbItem, view, mergeLevel);

			return mergeItem;
		}

		private MergeItemView CreateView(ItemDbInfo itemDbInfo)
		{
			var info = itemsLibrary.GetFactoryInfo(itemDbInfo.ID);
			var prefab = info.ItemPrefab;
			var viewObj = instantiator.Instantiate(prefab);
			var view = viewObj.GetComponent<MergeItemView>();

			return view;
		}
	}
}