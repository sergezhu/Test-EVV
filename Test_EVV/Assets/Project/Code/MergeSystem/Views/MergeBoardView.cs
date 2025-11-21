namespace Code.MergeSystem
{
	using System.Collections.Generic;
	using Code.Core;
	using Sirenix.OdinInspector;
	using UnityEngine;
	using Utilities.Extensions;

	public class MergeBoardView : MonoBehaviour, IInitializable
	{
		[SerializeField] private Transform cellsRoot;
		[SerializeField] private CellsBounds cellsBounds;
		[SerializeField] private List<MergeBoardCellView> cells;
		
		private MergeConfig mergeConfig;
		private BoardCellFactory cellFactory;

		public bool IsLocked { get; set; }

		public void Construct(MergeConfig mergeConfig, BoardCellFactory cellFactory)
		{
			this.mergeConfig = mergeConfig;
			this.cellFactory = cellFactory;
		}

		public void Initialize()
		{
			cells.ForEach(c => c.Initialize());
		}
		
		[Button]
		public void CreateCells()
		{
			cellsRoot.DestroyChildren();
			cells.Clear();
			
			Vector2 cellSize = mergeConfig.CellSize;
			Vector2 cellSpace = mergeConfig.CellSpace;
			
			for (int i = 0; i < mergeConfig.BoardSize.x; i++)
			for (int j = 0; j < mergeConfig.BoardSize.y; j++)
			{
				MergeBoardCellView cell = cellFactory.Create(cellsRoot);
				
				Vector3 cellPos = cellsBounds.CalculatePosition(i, j, mergeConfig.BoardSize, cellSize, cellSpace);
				cell.transform.position = cellPos;
				cell.transform.localScale = cellSize.x0y();
				
				cells.Add(cell);
			}
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