namespace Code.MergeSystem
{
	using Sirenix.OdinInspector;
	using Sirenix.Utilities;
	using UnityEngine;

	public class CellSelection : MonoBehaviour
	{
		[SerializeField] private CellSelectionHint[] _selectionCorners;

		public void Initialize()
		{
			_selectionCorners.ForEach( corner => corner.Initialize() );
		}

		public void SwitchToState( CellInteractionState state )
		{
			_selectionCorners.ForEach( c => c.SwitchToState( state ) );
		}

		[Button, HorizontalGroup("buttons1")]
		private void ToDefault() => SwitchToState( CellInteractionState.Default );
		
		[Button, HorizontalGroup( "buttons1" )]
		private void ToSuccess() => SwitchToState( CellInteractionState.Success );
		
		[Button, HorizontalGroup( "buttons1" )]
		private void ToFail() => SwitchToState( CellInteractionState.Fail );
		
		[Button, HorizontalGroup( "buttons1" )]
		private void ToOther() => SwitchToState( CellInteractionState.OtherMergeable );
	}
}