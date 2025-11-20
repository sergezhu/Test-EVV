namespace Code.Database
{
	using System.Collections.Generic;
	using System.Linq;
	using Sirenix.OdinInspector;
	using UnityEngine;

	[CreateAssetMenu(fileName = "ItemsLibrary", menuName = "Configs/Items/ItemsLibrary")]
	public class ItemsLibrary : SerializedScriptableObject
	{
		[ListDrawerSettings(ShowPaging = false)]
		[SerializeField] private List<DatabaseItemSO> _databaseItems;


		private DatabaseItem TryGetItemByID(uint id)
		{
			DatabaseItem dbItem = null;

			int index = _databaseItems.FindIndex(def => def.ID == id);
			bool isFounded = index != -1;

			if (isFounded) dbItem = _databaseItems[index].GetItem();

			return dbItem;
		}

		public IDatabaseItem TryGetItemByName(string itemName)
		{
			DatabaseItem dbItem = null;

			int index = _databaseItems.FindIndex(def => string.Equals(def.Name, itemName));
			bool isFounded = index != -1;

			if (isFounded) dbItem = _databaseItems[index].GetItem();

			return dbItem;
		}


		public ItemFactoryInfo GetFactoryInfo(uint itemId)
		{
			int index = _databaseItems.FindIndex(d => d.ID == itemId);
			DatabaseItemSO dbItem = _databaseItems[index];

			ItemFactoryInfo info = new ItemFactoryInfo
			{
				Info = new ItemDbInfo(dbItem.ID, dbItem.Name),
				ItemPrefab = dbItem.BoardItemPrefab
			};

			return info;
		}

		public IDatabaseItem GetDbItem(uint id)
		{
			return TryGetItemByID(id);
		}

		public IEnumerable<ItemDbInfo> GetAllItems()
		{
			return _databaseItems.Select(item => new ItemDbInfo(item.ID, item.Name));
		}

		public DatabaseItem GetItem(uint id)
		{
			return TryGetItemByID(id);
		}
	}
}