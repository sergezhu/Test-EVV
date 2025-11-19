namespace Code.MergeSystem
{
	using System;
	using Code.Database;
	using Sirenix.OdinInspector;

	[Serializable]
	public struct MergeItemInfo
	{
		[InlineProperty, HideLabel]
		public ItemDbInfo DbInfo;
		public int BuyCost;
		public int BoughtsCountToUpgrade;
		public float RewardPerHit;
	}
}