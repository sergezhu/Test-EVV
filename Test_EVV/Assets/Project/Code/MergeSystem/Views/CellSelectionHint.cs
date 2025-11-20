namespace Code.MergeSystem
{
	using UnityEngine;
	using Utilities.Extensions;

	public class CellSelectionHint : MonoBehaviour
	{
		[SerializeField] private GameObject successHitHintLayer;
		[SerializeField] private GameObject failHitHintLayer;
		[SerializeField] private GameObject otherMergeableHintLayer;

		private CellInteractionState interactionState;

		public void Initialize()
		{
			SetState(CellInteractionState.Default);
		}

		public void SwitchToState(CellInteractionState state)
		{
			if (state == interactionState)
				return;

			interactionState = state;

			SetState(state);
		}

		private void SetState(CellInteractionState interactionState)
		{
			switch (interactionState)
			{
				case CellInteractionState.Default:
					successHitHintLayer.Hide();
					failHitHintLayer.Hide();
					otherMergeableHintLayer.Hide();
					break;
				case CellInteractionState.Success:
					successHitHintLayer.Show();
					failHitHintLayer.Hide();
					otherMergeableHintLayer.Hide();
					break;
				case CellInteractionState.Fail:
					successHitHintLayer.Hide();
					failHitHintLayer.Show();
					otherMergeableHintLayer.Hide();
					break;
				case CellInteractionState.OtherMergeable:
					successHitHintLayer.Hide();
					failHitHintLayer.Hide();
					otherMergeableHintLayer.Show();
					break;
			}
		}
	}
}