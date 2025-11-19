namespace Code.MergeSystem
{
	using UnityEngine;

	public enum CellInteractionState
	{
		None,
		
		Default,
		Success,
		Fail,
		OtherMergeable
	}
	
	public class MergeBoardCellView : MonoBehaviour
	{
		[SerializeField] private Transform _spawnPoint;
		[SerializeField] private CellSelection _cellSelection;

		public Vector3 SpawnPosition => _spawnPoint.position;
		public Transform ItemsRoot => _spawnPoint;

		public void Initialize()
		{
			_cellSelection.Initialize();
		}

		public void SwitchToState( CellInteractionState state ) => _cellSelection.SwitchToState( state );
	}
}