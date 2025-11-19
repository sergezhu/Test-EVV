namespace Utilities.RxUI
{
	#if UNIRX
	
	using System;
	using UniRx;
	using UnityEngine;
	using UnityEngine.UI;

	public class UIToggle : MonoBehaviour
	{
		[SerializeField] private Toggle _toggle;
		
		public ReadOnlyReactiveProperty<bool> RxValue { get; private set; }

		
		public virtual void Initialize()
		{
			RxValue = _toggle.onValueChanged.AsObservable().ToReadOnlyReactiveProperty(_toggle.isOn);
		}

		public void Enable() => _toggle.enabled = true;
		public void Disable() => _toggle.enabled = false;
		public void SetOn() => _toggle.SetIsOnWithoutNotify( true );
		public void SetOff() => _toggle.SetIsOnWithoutNotify( false );
	}

	#endif
}