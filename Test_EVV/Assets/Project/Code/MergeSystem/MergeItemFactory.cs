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
			DatabaseItem dbItem = itemsLibrary.GetItem(itemDbInfo.ID);
			int mergeLevel = mergeConfig.GetMergeLevel(itemDbInfo.ID);
			MergeItemView view = CreateView(itemDbInfo);
			MergeItem mergeItem = new MergeItem(dbItem, view, mergeLevel);

			return mergeItem;
		}

		public MergeItem Create(int mergeLevel, Vector3 worldPos, Transform viewParent)
		{
			ItemDbInfo item = mergeConfig.GetMergeItem(mergeLevel);
			DatabaseItem dbItem = itemsLibrary.GetItem(item.ID);
			MergeItemView view = CreateView(item);
			view.SetInitialGeometry(worldPos, viewParent);
			MergeItem mergeItem = new MergeItem(dbItem, view, mergeLevel);

			return mergeItem;
		}

		public MergeItem Create(MergeItem referenceItem)
		{
			return Create(referenceItem.MergeLevel, referenceItem.Position, referenceItem.Parent);
		}

		private MergeItemView CreateView(ItemDbInfo itemDbInfo)
		{
			ItemFactoryInfo info = itemsLibrary.GetFactoryInfo(itemDbInfo.ID);
			MergeItemView prefab = info.ItemPrefab;
			MergeItemView viewObj = instantiator.Instantiate(prefab);
			MergeItemView view = viewObj.GetComponent<MergeItemView>();
			view.Construct(mergeConfig);

			return view;
		}
	}
}