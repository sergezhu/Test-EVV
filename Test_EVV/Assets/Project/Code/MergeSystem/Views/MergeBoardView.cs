namespace Code.MergeSystem
{
	using System.Collections.Generic;
	using Code.Core;
	using UnityEngine;

	public class MergeBoardView : MonoBehaviour, IInitializable
	{
		[SerializeField] private Transform cellsRoot;
		[SerializeField] private List<MergeBoardCellView> cells;

		public bool IsLocked { get; set; }

		public void Initialize()
		{
			cells.ForEach(c => c.Initialize());
		}

		public Vector3 GetSpawnPosition(int cellIndex)
		{
			return cells[cellIndex].SpawnPosition;
		}

		public Transform GetItemRoot(int cellIndex)
		{
			return cells[cellIndex].ItemsRoot;
		}

		public void SwitchToState(int cellIndex, CellInteractionState state)
		{
			cells[cellIndex].SwitchToState(state);
		}

		public void SwitchToState(MergeBoardCellView cell, CellInteractionState state)
		{
			if (cell == null)
				return;

			cell.SwitchToState(state);
		}

		public void DisableCellsHints()
		{
			cells.ForEach(cell => cell.SwitchToState(CellInteractionState.Default));
		}

		public int GetCellIndex(MergeBoardCellView cellView)
		{
			return cells.FindIndex(cell => cell == cellView);
		}
	}
}