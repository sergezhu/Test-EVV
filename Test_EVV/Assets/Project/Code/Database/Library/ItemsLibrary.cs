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

			var index = _databaseItems.FindIndex(def => def.ID == id);
			var isFounded = index != -1;

			if (isFounded) dbItem = _databaseItems[index].GetItem();

			return dbItem;
		}

		private DatabaseItem TryGetItemByName(string name)
		{
			DatabaseItem dbItem = null;

			var index = _databaseItems.FindIndex(def => string.Equals(def.Name, name));
			var isFounded = index != -1;

			if (isFounded) dbItem = _databaseItems[index].GetItem();

			return dbItem;
		}


		public ItemFactoryInfo GetFactoryInfo(uint itemId)
		{
			var index = _databaseItems.FindIndex(d => d.ID == itemId);
			var dbItem = _databaseItems[index];

			var info = new ItemFactoryInfo
			{
				Info = new ItemDbInfo
				{
					ID = dbItem.ID,
					Name = dbItem.Name
				},
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
			return _databaseItems
				.Select(item => new ItemDbInfo { ID = item.ID, Name = item.Name });
		}

		public DatabaseItem GetItem(uint id)
		{
			return TryGetItemByID(id);
		}
	}
}