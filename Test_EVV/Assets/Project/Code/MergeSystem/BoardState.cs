namespace Code.MergeSystem
{
	using System.Collections.Generic;
	using System.Linq;

	public sealed class BoardState
	{
		private List<MergeBoardCellRecord> mergeBoardRestoreData;
		private MergeStatistic mergeStatistic;

		public BoardState(int currentMergeLevel, MergeStatistic mergeStatistic, List<MergeBoardCellRecord> mergeBoardRestoreData)
		{
			CurrentMergeLevel = currentMergeLevel;

			this.mergeStatistic = mergeStatistic;
			this.mergeBoardRestoreData = mergeBoardRestoreData;
		}

		public int CurrentMergeLevel { get; }

		public MergeStatistic MergeStatistic => mergeStatistic;

		public IReadOnlyList<MergeBoardCellRecord> MergeBoardRestoreData => mergeBoardRestoreData;


		public void AddBuyedToStats(int mergeLevel)
		{
			mergeStatistic.AddBuyed(mergeLevel);
		}

		public void AddMergedToStats(int mergeLevel)
		{
			mergeStatistic.AddMerged(mergeLevel);
		}

		public void AddUpgradeCountToStats()
		{
			mergeStatistic.AddUpgradeCount();
		}

		public void UpdateRestoreData(IEnumerable<MergeBoardCellRecord> restoreData)
		{
			mergeBoardRestoreData = restoreData.ToList();
		}
	}
}