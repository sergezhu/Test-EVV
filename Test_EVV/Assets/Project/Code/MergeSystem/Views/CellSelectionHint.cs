namespace Code.MergeSystem
{
	using UnityEngine;
	using Utilities.Extensions;

	public class CellSelectionHint : MonoBehaviour
	{

		[SerializeField] private GameObject _successHitHintLayer;
		[SerializeField] private GameObject _failHitHintLayer;
		[SerializeField] private GameObject _otherMergeableHintLayer;

		private CellInteractionState _interactionState;

		public void Initialize()
		{
			SetState( CellInteractionState.Default );
		}

		public void SwitchToState( CellInteractionState state )
		{
			if ( state == _interactionState )
				return;

			_interactionState = state;

			SetState( state );
		}

		private void SetState( CellInteractionState interactionState )
		{
			switch ( interactionState )
			{
				case CellInteractionState.Default:
					_successHitHintLayer.Hide();
					_failHitHintLayer.Hide();
					_otherMergeableHintLayer.Hide();
					break;
				case CellInteractionState.Success:
					_successHitHintLayer.Show();
					_failHitHintLayer.Hide();
					_otherMergeableHintLayer.Hide();
					break;
				case CellInteractionState.Fail:
					_successHitHintLayer.Hide();
					_failHitHintLayer.Show();
					_otherMergeableHintLayer.Hide();
					break;
				case CellInteractionState.OtherMergeable:
					_successHitHintLayer.Hide();
					_failHitHintLayer.Hide();
					_otherMergeableHintLayer.Show();
					break;
			}
		}
	}
}