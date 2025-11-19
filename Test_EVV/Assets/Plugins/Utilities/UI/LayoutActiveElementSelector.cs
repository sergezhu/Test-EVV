namespace Utilities.UI
{
	using System.Collections;
	using System.Linq;
	using UniRx;
	using UnityEngine;
	using Utilities.RxUI;

	public class LayoutActiveElementSelector : MonoBehaviour
	{
		[SerializeField] private AdaptiveHorizontalLayoutGroup _adaptiveGroup;
		
		[SerializeField] private bool autoActivateOnPointerEnter = false;
		[SerializeField] private bool autoDeactivateOnPointerExit = false;
		[SerializeField] private bool autoActivateOnPointerClick = false;

		[Header( "Debug" )]
		[SerializeField] private bool _initializeOnAwake;
		[SerializeField] private UIBaseButton _activeInteractableButton;
		[SerializeField] private UIBaseButton[] _interactableChildren;
		
		private Coroutine _activatingRoutine;

		public bool AutoActivateOnPointerEnter => autoActivateOnPointerEnter;
		public bool AutoDeactivateOnPointerExit => autoDeactivateOnPointerExit;
		public bool AutoActivateOnPointerClick => autoActivateOnPointerClick;

		private void Awake()
		{
			if ( _initializeOnAwake )
			{
				Initialize();
			}
		}

		public void Initialize()
		{
			var allMonoBehaviours = _adaptiveGroup.GetComponentsInChildren<MonoBehaviour>( false );
			var baseButtons = allMonoBehaviours.OfType<UIBaseButton>().ToArray();
			
			_interactableChildren = baseButtons;
			_adaptiveGroup.ActiveElement = null;

			foreach ( var child in _interactableChildren )
			{
				child.Initialize();

				child.Click.Where( _ => AutoActivateOnPointerClick )
					.Subscribe( _ => ActivateChildOnClick( child ) )
					.AddTo( this );

				child.IsOver.Where( isOver => isOver && AutoActivateOnPointerEnter )
					.Subscribe( _ => ActivateChildOnPointerEnter( child ) )
					.AddTo( this );

				child.IsOver.Where( isOver => !isOver && AutoDeactivateOnPointerExit )
					.Subscribe( _ => DeactivateChildOnPointerExit( child ) )
					.AddTo( this );
			}
		}

		private void ActivateChildOnPointerEnter( UIBaseButton child )
		{
			_activeInteractableButton = child;
			
			if( _activatingRoutine != null )
				StopCoroutine( _activatingRoutine );
			
			_activatingRoutine = StartCoroutine( SetActiveChildRoutine( _activeInteractableButton ) );
		}

		private void DeactivateChildOnPointerExit( UIBaseButton child )
		{
			_activeInteractableButton = null;

			if ( _activatingRoutine != null )
				StopCoroutine( _activatingRoutine );
			
			_activatingRoutine = StartCoroutine( SetActiveChildRoutine( _activeInteractableButton ) );
		}

		private void ActivateChildOnClick( UIBaseButton child )
		{
			_activeInteractableButton = child;

			if ( _activatingRoutine != null )
				StopCoroutine( _activatingRoutine );
			
			_activatingRoutine = StartCoroutine( SetActiveChildRoutine( _activeInteractableButton ) );

		}

		private IEnumerator SetActiveChildRoutine( UIBaseButton child )
		{
			if ( _adaptiveGroup.IsAnimating )
				yield return null;

			_adaptiveGroup.ActiveElement = child != null ? child.RectTransform : null;
		}
	}
}