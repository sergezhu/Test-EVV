namespace Code.Database
{
	using UnityEngine;

	[CreateAssetMenu(fileName = "Common Item", menuName = "Configs/Items/Database/ComonItem")]
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