namespace Code.Database
{
	using System;
	using Code.MergeSystem;
	using UnityEngine;

	[Serializable]
	public abstract class DatabaseItem : IDatabaseItem
	{
		[SerializeField] private uint id;
		[SerializeField] private string name;
		[SerializeField] private Sprite icon;
		[SerializeField] private MergeItemView itemPrefab;


		public DatabaseItem(uint id, string name, Sprite icon)
		{
			this.id = id;
			this.name = name;
			this.icon = icon;
		}

		public Sprite Icon => icon;
		public MergeItemView ItemPrefab => itemPrefab;


		public uint ID => id;
		public string Name => name;

		public void ValidateID(uint id)
		{
			this.id = id;
		}
	}
}