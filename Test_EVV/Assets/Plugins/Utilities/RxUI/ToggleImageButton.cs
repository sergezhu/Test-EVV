namespace Utilities.RxUI
{
	#if UNIRX
	
	using System;
	using System.Collections.Generic;
	using UniRx;
	using UnityEngine;
	using Utilities.Extensions;

	public class ToggleImageButton : RectTransformOwner
	{
		[SerializeField] protected UIImageButton _button1;
		[SerializeField] protected UIImageButton _button2;
		
		private List<UIBaseButton> _buttons;
		private CompositeDisposable _internalDisposable;
		public ReactiveCommand Click { get; } = new ReactiveCommand();
		public int CurrentIndex { get; private set; }

		
		public virtual void Initialize()
		{
			_internalDisposable = new CompositeDisposable();
			_buttons = new List<UIBaseButton> { _button1, _button2 };

			CurrentIndex = 0;
			UpdateButtons();

			_buttons.ForEach( (b, i) =>
			{
				b.Initialize();
				
				b.Click
					.Subscribe( _ => OnClick() )
					.AddTo( _internalDisposable );
			} );
		}

		public void Enable()
		{
			_button1.IsEnabled.Value = true;
			_button2.IsEnabled.Value = true;
		}

		public void Disable()
		{
			_button1.IsEnabled.Value = false;
			_button2.IsEnabled.Value = false;
		}

		public void SetButton1Icon( Sprite icon ) => _button1.SetIcon( icon );

		public void ClearButton1Icon() => _button1.ClearIcon();

		public void SetButton2Icon( Sprite icon ) => _button2.SetIcon( icon );

		public void ClearButton2Icon() => _button2.ClearIcon();

		public void Toggle()
		{
			CurrentIndex = (CurrentIndex + 1) % _buttons.Count;
			UpdateButtons();
		}

		public void SetIndex( int index )
		{
			if ( index >= _buttons.Count )
				throw new InvalidOperationException();
			
			CurrentIndex = index;
			UpdateButtons();
		}

		private void UpdateButtons()
		{
			for ( var i = 0; i < _buttons.Count; i++ )
			{
				if(CurrentIndex == i)
					_buttons[i].Show();
				else
					_buttons[i].Hide();
			}
		}

		private void OnClick()
		{
			Toggle();
			Click.Execute();
		}
	}

	#endif
}