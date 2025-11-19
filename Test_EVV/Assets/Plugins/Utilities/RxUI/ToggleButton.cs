namespace Utilities.RxUI
{
	#if UNIRX
	
	using System.Collections.Generic;
	using UniRx;
	using UnityEngine;
	using Utilities.Extensions;

	public class ToggleButton : MonoBehaviour
	{
		[SerializeField] protected UIBaseButton _button1;
		[SerializeField] protected UIBaseButton _button2;
		
		private List<UIBaseButton> _buttons;
		private CompositeDisposable _internalDisposable;
		public ReactiveCommand Click { get; } = new ReactiveCommand();
		public int CurrentIndex { get; private set; }

		
		public virtual void Initialize()
		{
			_internalDisposable = new CompositeDisposable();
			_buttons = new List<UIBaseButton> { _button1, _button2 };

			CurrentIndex = 0;
			SwitchToIndex();

			_buttons.ForEach( (b, i) =>
			{
				b.Initialize();
				
				b.Click
					.Subscribe( _ =>
					{
						CurrentIndex = (CurrentIndex + 1) % _buttons.Count;
						SwitchToIndex();
						Click.Execute();
					} )
					.AddTo( _internalDisposable );
			} );
		}

		private void SwitchToIndex()
		{
			for ( var i = 0; i < _buttons.Count; i++ )
			{
				if(CurrentIndex == i)
					_buttons[i].Show();
				else
					_buttons[i].Hide();
			}
		}
	}

	#endif
}