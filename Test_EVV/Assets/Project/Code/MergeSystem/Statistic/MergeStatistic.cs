namespace Code.MergeSystem
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	[Serializable]
	public struct MergeStatistic
	{
		[SerializeField] private int _upgradeCount;
		[SerializeField] private List<MergeLevelStatistic> _mergeLevelStatistics;


		public int UpgradeCount => _upgradeCount;

		public void Initialize()
		{
			_mergeLevelStatistics = new List<MergeLevelStatistic>();
			_upgradeCount = 0;
		}
		

		public MergeLevelStatistic? GetStatistic( int mergeLevel )
		{
			if ( TryFindStatisticIndex( mergeLevel, out var index ) )
			{
				return _mergeLevelStatistics[index];
			}

			return null;
		}

		public bool TryFindStatisticIndex( int mergeLevel, out int statisticIndex )
		{
			statisticIndex = _mergeLevelStatistics.FindIndex( stat => stat.MergeLevel == mergeLevel );
			return statisticIndex != -1;
		}

		public void AddMerged( int mergeLevel )
		{
			if ( TryFindStatisticIndex( mergeLevel, out var statisticIndex ) == false )
			{
				AddNewRecord( mergeLevel );
			};

			TryFindStatisticIndex( mergeLevel, out statisticIndex );
			var stat = _mergeLevelStatistics[statisticIndex];
			stat.MergedCount += 1;
			_mergeLevelStatistics[statisticIndex] = stat;
		}

		public void AddBuyed( int mergeLevel )
		{
			if ( TryFindStatisticIndex( mergeLevel, out var statisticIndex ) == false )
			{
				AddNewRecord( mergeLevel );
			}

			TryFindStatisticIndex( mergeLevel, out statisticIndex );
			var stat = _mergeLevelStatistics[statisticIndex];
			stat.BuyedCount += 1;
			_mergeLevelStatistics[statisticIndex] = stat;
		}

		private void AddNewRecord( int mergeLevel )
		{
			var newRecord = new MergeLevelStatistic() { MergeLevel = mergeLevel };
			_mergeLevelStatistics.Add( newRecord );
		}

		public void AddUpgradeCount()
		{
			_upgradeCount += 1;
		}
	}
}