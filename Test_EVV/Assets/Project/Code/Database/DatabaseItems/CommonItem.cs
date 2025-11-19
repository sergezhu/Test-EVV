namespace Code.Database
{
	using System;
	using UnityEngine;

	[Serializable]
	public class CommonItem : DatabaseItem
	{
		[SerializeField] private CommonItemAttributes _attributes;


		public CommonItem( uint id, string name, CommonItemAttributes attributes, Sprite icon ) : base( id, name, icon )
		{
			_attributes = attributes;
		}

		public CommonItemAttributes Attributes => _attributes;
	}
}	