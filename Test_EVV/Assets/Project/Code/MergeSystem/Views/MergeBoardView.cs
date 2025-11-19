namespace Code.MergeSystem
{
	using System.Collections.Generic;
	using Code.Core;
	using UnityEngine;

	public class MergeBoardView : MonoBehaviour, IInitializable
	{
		[SerializeField] private Transform _cellsRoot;
		[SerializeField] private List<MergeBoardCellView> _cells;
		
		public bool IsLocked { get; set; }

		public void Initialize()
		{
			_cells.ForEach( c => c.Initialize() );
		}

		public Vector3 GetSpawnPosition( int cellIndex )
		{
			return _cells[cellIndex].SpawnPosition;
		}

		public Transform GetItemRoot( int cellIndex )
		{
			return _cells[cellIndex].ItemsRoot;
		}

		public void SwitchToState( int cellIndex, CellInteractionState state )
		{
			_cells[cellIndex].SwitchToState( state );
		}

		public void SwitchToState( MergeBoardCellView cell, CellInteractionState state )
		{
			if(cell == null)
				return;
			
			cell.SwitchToState( state );
		}

		public void DisableCellsHints()
		{
			_cells.ForEach( cell => cell.SwitchToState( CellInteractionState.Default ) );
		}

		public int GetCellIndex( MergeBoardCellView cellView )
		{
			return _cells.FindIndex( cell => cell == cellView );
		}
	}
}