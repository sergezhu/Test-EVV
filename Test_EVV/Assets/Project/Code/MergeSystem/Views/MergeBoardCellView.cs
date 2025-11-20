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
		[SerializeField] private Transform spawnPoint;
		[SerializeField] private CellSelection cellSelection;

		public Vector3 SpawnPosition => spawnPoint.position;
		public Transform ItemsRoot => spawnPoint;

		public void Initialize()
		{
			cellSelection.Initialize();
		}

		public void SwitchToState(CellInteractionState state)
		{
			cellSelection.SwitchToState(state);
		}
	}
}