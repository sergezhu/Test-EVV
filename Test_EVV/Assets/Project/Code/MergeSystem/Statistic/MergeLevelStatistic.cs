namespace Code.MergeSystem
{
	using System;

	[Serializable]
	public struct MergeLevelStatistic
	{
		public int MergeLevel;
		public int MergedCount;
		public int BuyedCount;

		public int CreatedCount => MergedCount + BuyedCount;
	}
}