namespace Utilities.RxUI
{
	using System;
	using Sirenix.OdinInspector;
	using UniRx;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;
	using Utilities.Extensions;

	public class UIBaseButton : RectTransformOwner, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public const float DoubleClickInterval = 250f;

		[Space]
		[SerializeField] private bool _doubleClickEnabled;
		[SerializeField, ReadOnly] private bool _isEnabled;
		
		[Space]
		[SerializeField] private GameObject _defaultView;
		[SerializeField] private GameObject _selectedView;
		[SerializeField] private GameObject _disabledView;
		[SerializeField] private GameObject _highlightedHint;
		
		private Button _nativeButton;

		public bool IsInitialized { get; private set; }

		public ReactiveCommand Down { get; } = new ReactiveCommand();
		public ReactiveCommand Up { get; } = new ReactiveCommand();

		public ReactiveProperty<bool> IsOver { get; } = new ReactiveProperty<bool>();
		public ReactiveProperty<bool> IsPressed { get; } = new ReactiveProperty<bool>();
		public ReactiveProperty<bool> IsSelected { get; } = new ReactiveProperty<bool>();
		public ReactiveProperty<bool> IsEnabled { get; } = new ReactiveProperty<bool>();
		
		public bool LockSingleClick { get; set; }
		public bool LockDoubleClick { get; set; }


		public ReactiveCommand Click { get; } = new ReactiveCommand();
		public ReactiveCommand DoubleClick { get; } = new ReactiveCommand();
		
		private Subject<Unit> _clickStream = new Subject<Unit>();
		
		protected bool DraggingFlag { get; set; }
		
		private int ClicksCount { get; set; }
		private float ClicksTimer { get; set; }
		private bool DoubleClickEnabled => _doubleClickEnabled;

		protected GameObject DisabledView => _disabledView;
		protected GameObject DefaultView => _defaultView;


		public new void Initialize()
		{
			if(IsInitialized)
				Debug.LogWarning( $"{name} is already initialized but you repeat it" );

			base.Initialize();
			
			TryGetComponent( out _nativeButton );
			
			IsInitialized = true;
			LockSingleClick = false;
			LockDoubleClick = false;
			
			SubscribeInternal();

			IsEnabled.Value = true;
			IsSelected.Value = false;

			CustomInitialize();
		}

		protected virtual void CustomInitialize()
		{
		}

		public void Update()
		{
			UpdateClicksTimer();
		}

		public void OnPointerClick( PointerEventData eventData )
		{
			if ( !IsEnabled.Value )
				return;

			_clickStream.OnNext( Unit.Default );
		}

		public void OnPointerDown( PointerEventData eventData )
		{
			if ( !IsEnabled.Value )
				return;

			//Debug.Log( $"OnPointerDownHandler : {this.name}" );
			IsPressed.Value = true;
			Down.Execute();
		}

		public void OnPointerUp( PointerEventData eventData )
		{
			if ( !IsEnabled.Value )
				return;

			//Debug.Log( $"OnPointerUpHandler : {this.name}" );
			IsPressed.Value = false;
			Up.Execute();
		}

		public void OnPointerEnter( PointerEventData eventData )
		{
			if ( !IsEnabled.Value )
				return;

			IsOver.Value = true;
			//ShowHighlight();
		}

		public void OnPointerExit( PointerEventData eventData )
		{
			if ( !IsEnabled.Value )
				return;

			IsOver.Value = false;
			//HideHighlight();
		}

		public void Show( bool force = false, Action onComplete = null )
		{
			gameObject.SetActive( true );
			onComplete?.Invoke();
		}

		public void Hide( bool force = false, Action onComplete = null )
		{
			IsPressed.Value = false;
			gameObject.SetActive( false );
			onComplete?.Invoke();
		}

		public void ShowHighlight()
		{
			if ( _highlightedHint != null )
				_highlightedHint.Show();
		}

		public void HideHighlight()
		{
			if ( _highlightedHint != null )
				_highlightedHint.Hide();
		}

		protected virtual bool ClickCondition()
		{
			return true;
		}

		protected virtual bool PressedCondition()
		{
			return true;
		}

		private void SubscribeInternal()
		{
			IsSelected
				.Where( _ => IsEnabled.Value )
				.Subscribe( UpdateSelectedState )
				.AddTo( this );

			IsEnabled
				.Subscribe( UpdateEnabledState )
				.AddTo( this );

			_clickStream
				.Where( _ => ClickCondition() )
				.Subscribe( _ =>
				{
					if ( DraggingFlag )
					{
						DraggingFlag = false;
					}
					else
					{
						if ( DoubleClickEnabled )
						{
							ClicksCount++;

							if ( ClicksCount == 1 )
								ClicksTimer = DoubleClickInterval * 0.001f;
						}
						else
						{
							if ( LockSingleClick == false )
								Click.Execute();
						}
					}
				} )
				.AddTo( this );

			/*_clickStream
				.Where( _ => ClickCondition() )
				.Buffer( _clickStream.Throttle( TimeSpan.FromMilliseconds( DoubleClickInterval ) ) )
				.Subscribe( clicks =>
				{
					if ( DraggingFlag )
					{
						DraggingFlag = false;
					}
					else
					{
						switch ( clicks.Count )
						{
							case 1:
								Click.Execute();
								break;

							case 2:
								DoubleClick.Execute();
								break;
						}
					}
				} ) 
				.AddTo( this );*/
		}

		private void UpdateClicksTimer()
		{
			if(_doubleClickEnabled == false)
				return;
			
			if ( ClicksTimer > 0 )
			{
				ClicksTimer -= Time.deltaTime;

				if ( ClicksTimer < 0 )
				{
					switch ( ClicksCount )
					{
						case 1:
							if(LockSingleClick == false)
								Click.Execute();
							break;

						case 2:
							if(LockDoubleClick == false)
								DoubleClick.Execute();
							break;
					}

					ClicksTimer = 0;
					ClicksCount = 0;
				}
			}
			else
			{
				/*switch ( ClicksCount )
				{
					case 1:
						Click.Execute();
						break;

					case 2:
						DoubleClick.Execute();
						break;
				}

				ClicksTimer = 0;
				ClicksCount = 0;*/
			}
		}

		private void UpdateSelectedState( bool v )
		{
			Debug.Log( $"UpdateSelectedState : {v}" );
			
			if(_selectedView != null)
				_selectedView.SetActive( v );
			
			if ( _defaultView != null )
				_defaultView.SetActive( !v );
		}

		private void UpdateEnabledState( bool v )
		{
			_isEnabled = v;
			
			if ( _nativeButton != null )
				_nativeButton.interactable = v;
			
			if ( v == false )
			{
				IsSelected.Value = false;
				IsPressed.Value = false;

				if ( _defaultView != null )
					_defaultView.SetActive( false );
				
				if(_selectedView != null)
					_selectedView.SetActive( false );
				
				HideHighlight();
			}
			else
			{
				if ( _defaultView != null )
					_defaultView.SetActive( !IsSelected.Value );

				if ( _selectedView != null )
					_selectedView.SetActive( IsSelected.Value );
			}

			if( _disabledView != null)
				_disabledView.SetActive( !v );
		}
	}
}