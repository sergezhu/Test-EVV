namespace Code.MergeSystem
{
	using Code.Core;
	using Code.Database;
	using UniRx;
	using UnityEngine;

	public struct MergeItemReceiveOptions
	{
		public bool CanPlayReceiveFX;
	}

	public class MergeItemReceiver : MonoBehaviour, IInitializable
	{
		private MergeItemReceiverFacade facade;
		private ItemsLibrary itemsLibrary;
		private MergeConfig mergeConfig;
		
		public ReadOnlyReactiveProperty<ItemDbInfo> CurrentItem { get; private set; }
		public ReadOnlyReactiveProperty<DatabaseItem> CurrentDatabaseItem { get; private set; }
		public MergeItemReceiveOptions ReceiveOptions { get; private set; }		
		public bool HasItem => CurrentItem.Value.ID != 0;
		public Vector3 Position => transform.position;

		private ReactiveProperty<ItemDbInfo> CurrentItemInternal { get; } = new ReactiveProperty<ItemDbInfo>();
		private ReactiveProperty<DatabaseItem> CurrentDatabaseItemInternal { get; } = new ReactiveProperty<DatabaseItem>();


		private void OnDestroy()
		{
			facade.Receiver = null;
		}

		private void OnValidate()
		{
			if (gameObject.layer != mergeConfig.MergeItemReceiverLayer)
				gameObject.layer = mergeConfig.MergeItemReceiverLayer;
		}

		public void Initialize()
		{
			CurrentItem = CurrentItemInternal.ToReadOnlyReactiveProperty();
			CurrentDatabaseItem = CurrentDatabaseItemInternal.ToReadOnlyReactiveProperty();

			facade.Receiver = this;

			/*if ( _heroEquip.ActiveWeapon.Value != null )
			{
				CurrentItemInternal.Value = (InventoryItem)_heroEquip.ActiveWeapon.Value;
			}*/
		}


		public void Construct(MergeConfig mergeConfig, ItemsLibrary itemsLibrary, MergeItemReceiverFacade facade)
		{
			this.mergeConfig = mergeConfig;
			this.itemsLibrary = itemsLibrary;
			this.facade = facade;
		}

		public void ReceiveItem(ItemDbInfo item, MergeItemReceiveOptions options)
		{
			ReceiveOptions = options;

			var currentItemLevel = mergeConfig.GetMergeLevel(CurrentItemInternal.Value.ID);
			var targetItemLevel = mergeConfig.GetMergeLevel(item.ID);

			if (targetItemLevel <= currentItemLevel)
			{
				Debug.LogWarning("ReceiveItem : target merge level lesser or equal an current");
				return;
			}

			CurrentItemInternal.Value = item;

			Debug.Log($"ReceiveIitem : {item.Name}");

			var weapon = CurrentItem.Value.ID != 0
				? itemsLibrary.GetItem(CurrentItem.Value.ID)
				: null;

			CurrentDatabaseItemInternal.Value = weapon;
		}

		public void ReceiveDatabaseItem(DatabaseItem dbItem, MergeItemReceiveOptions options)
		{
			ReceiveOptions = options;

			CurrentDatabaseItemInternal.Value = dbItem;

			var item = dbItem == null ? default : new ItemDbInfo(dbItem.ID, dbItem.Name);
			CurrentItemInternal.Value = item;

			Debug.Log($"Receive DB item : {item.Name}");
		}

		public bool CanSwapItem(ItemDbInfo item)
		{
			var currentItemLevel = mergeConfig.GetMergeLevel(CurrentItemInternal.Value.ID);
			var targetItemLevel = mergeConfig.GetMergeLevel(item.ID);

			return targetItemLevel > currentItemLevel;
		}

		public bool CanMergeItem(ItemDbInfo item)
		{
			var currentItemLevel = mergeConfig.GetMergeLevel(CurrentItemInternal.Value.ID);
			var targetItemLevel = mergeConfig.GetMergeLevel(item.ID);

			return targetItemLevel == currentItemLevel;
		}

		public int GetCurrentMergeLevel()
		{
			if (CurrentItemInternal.Value.ID == 0)
				return -1;

			return mergeConfig.GetMergeLevel(CurrentItemInternal.Value.ID);
		}
	}
}