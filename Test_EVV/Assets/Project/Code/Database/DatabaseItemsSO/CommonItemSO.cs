namespace Code.Database
{
	using System;
	using UnityEngine;

	[Serializable]
	public class CommonItemSO : DatabaseItemSO
	{
		[SerializeField] private CommonItemAttributes _CommonItemAttributes;


		protected CommonItemAttributes CommonItemAttributes => _CommonItemAttributes;

		public override DatabaseItem GetItem()
		{
			return new CommonItem( ID, Name, CommonItemAttributes, Icon );
		}
	}
}	