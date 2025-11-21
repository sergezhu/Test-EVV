namespace Code.MergeSystem
{
	using Code.Core;
	using Code.Database;
	using UnityEngine;

	public class BoardCellFactory
	{
		private readonly IInstantiator instantiator;
		private readonly MergeConfig mergeConfig;
		private readonly ItemsLibrary itemsLibrary;

		public BoardCellFactory(MergeConfig mergeConfig, ItemsLibrary itemsLibrary, IInstantiator instantiator)
		{
			this.mergeConfig = mergeConfig;
			this.itemsLibrary = itemsLibrary;
			this.instantiator = instantiator;
		}

		public MergeBoardCellView CreateCellView(Transform parent)
		{
			MergeBoardCellView cellView = instantiator.Instantiate(mergeConfig.BoardCellPrefab, parent);
			cellView.SwitchToState(CellInteractionState.Default);
			
			return cellView;
		}

		public MergeItem CreateItem(int mergeLevel, Vector3 worldPos, Transform viewParent)
		{
			ItemDbInfo item = mergeConfig.GetMergeItem(mergeLevel);
			DatabaseItem dbItem = itemsLibrary.GetItem(item.ID);
			MergeItemView view = CreateItemView(item);
			view.SetInitialGeometry(worldPos, viewParent);
			MergeItem mergeItem = new MergeItem(dbItem, view, mergeLevel);

			return mergeItem;
		}

		private MergeItemView CreateItemView(ItemDbInfo itemDbInfo)
		{
			ItemFactoryInfo info = itemsLibrary.GetFactoryInfo(itemDbInfo.ID);
			MergeItemView prefab = info.ItemPrefab;
			MergeItemView viewObj = instantiator.Instantiate(prefab);
			MergeItemView view = viewObj.GetComponent<MergeItemView>();

			return view;
		}
	}
}