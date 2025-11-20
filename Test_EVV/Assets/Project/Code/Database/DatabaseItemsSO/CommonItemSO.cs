namespace Code.Database
{
	using System;
	using UnityEngine;

	[Serializable]
	public class CommonItemSO : DatabaseItemSO
	{
		[SerializeField] private CommonItemAttributes commonItemAttributes;


		protected CommonItemAttributes CommonItemAttributes => commonItemAttributes;

		public override DatabaseItem GetItem()
		{
			return new CommonItem(ID, Name, CommonItemAttributes, Icon);
		}
	}
}