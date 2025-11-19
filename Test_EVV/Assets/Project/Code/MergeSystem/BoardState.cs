namespace Code.MergeSystem
{
	using System.Collections.Generic;
	using System.Linq;

	public sealed class BoardState
	{
		private List<MergeBoardCellRecord> _mergeBoardRestoreData;
		private MergeStatistic _mergeStatistic;

		public BoardState( int currentMergeLevel, MergeStatistic mergeStatistic, List<MergeBoardCellRecord> mergeBoardRestoreData )
		{
			CurrentMergeLevel = currentMergeLevel;

			_mergeStatistic = mergeStatistic;
			_mergeBoardRestoreData = mergeBoardRestoreData;
		}

		public int CurrentMergeLevel { get; }

		public MergeStatistic MergeStatistic => _mergeStatistic;

		public IReadOnlyList<MergeBoardCellRecord> MergeBoardRestoreData => _mergeBoardRestoreData;


		public void AddBuyedToStats( int mergeLevel )
		{
			_mergeStatistic.AddBuyed( mergeLevel );
		}

		public void AddMergedToStats( int mergeLevel )
		{
			_mergeStatistic.AddMerged( mergeLevel );
		}

		public void AddUpgradeCountToStats()
		{
			_mergeStatistic.AddUpgradeCount();
		}

		public void UpdateRestoreData( IEnumerable<MergeBoardCellRecord> restoreData )
		{
			_mergeBoardRestoreData = restoreData.ToList();
		}
	}
}