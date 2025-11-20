namespace Code.MergeSystem
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	[Serializable]
	public struct MergeStatistic
	{
		[SerializeField] private int upgradeCount;
		[SerializeField] private List<MergeLevelStatistic> mergeLevelStatistics;


		public int UpgradeCount => upgradeCount;

		public void Initialize()
		{
			mergeLevelStatistics = new List<MergeLevelStatistic>();
			upgradeCount = 0;
		}


		public MergeLevelStatistic? GetStatistic(int mergeLevel)
		{
			if (TryFindStatisticIndex(mergeLevel, out int index)) return mergeLevelStatistics[index];

			return null;
		}

		public bool TryFindStatisticIndex(int mergeLevel, out int statisticIndex)
		{
			statisticIndex = mergeLevelStatistics.FindIndex(stat => stat.MergeLevel == mergeLevel);
			return statisticIndex != -1;
		}

		public void AddMerged(int mergeLevel)
		{
			if (TryFindStatisticIndex(mergeLevel, out int statisticIndex) == false) AddNewRecord(mergeLevel);
			;

			TryFindStatisticIndex(mergeLevel, out statisticIndex);
			MergeLevelStatistic stat = mergeLevelStatistics[statisticIndex];
			stat.MergedCount += 1;
			mergeLevelStatistics[statisticIndex] = stat;
		}

		public void AddBuyed(int mergeLevel)
		{
			if (TryFindStatisticIndex(mergeLevel, out int statisticIndex) == false) AddNewRecord(mergeLevel);

			TryFindStatisticIndex(mergeLevel, out statisticIndex);
			MergeLevelStatistic stat = mergeLevelStatistics[statisticIndex];
			stat.BuyedCount += 1;
			mergeLevelStatistics[statisticIndex] = stat;
		}

		private void AddNewRecord(int mergeLevel)
		{
			MergeLevelStatistic newRecord = new MergeLevelStatistic { MergeLevel = mergeLevel };
			mergeLevelStatistics.Add(newRecord);
		}

		public void AddUpgradeCount()
		{
			upgradeCount += 1;
		}
	}
}