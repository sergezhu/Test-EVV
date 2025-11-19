namespace Code.Database
{
	using System;
	using Code.MergeSystem;
	using UnityEngine;

	[Serializable]
	public abstract class DatabaseItem : IDatabaseItem
	{
		[SerializeField] private uint _id;
		[SerializeField] private string _name;
		[SerializeField] private Sprite _icon;
		[SerializeField] private MergeItemView _itemPrefab;


		public DatabaseItem( uint id, string name, Sprite icon )
		{
			_id = id;
			_name = name;
			_icon = icon;
		}


		public uint ID => _id;
		public string Name => _name;
		public Sprite Icon => _icon;
		public MergeItemView ItemPrefab => _itemPrefab;

		public void ValidateID( uint id )
		{
			_id = id;
		}
	}
}