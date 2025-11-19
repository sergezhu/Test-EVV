namespace Utilities.Extensions
{
	#if UNIRX
	
	using System;
	using UniRx;
	using UnityEngine.InputSystem;

	public static class RxNewInputSystemExtensions
	{
		public static IObservable<InputAction.CallbackContext> PerformedAsObservable( this InputAction inputAction ) =>
			Observable.FromEvent<InputAction.CallbackContext>
			(
				addHandler: action => inputAction.performed += action,
				removeHandler: action => inputAction.performed -= action
			);

		public static IObservable<InputAction.CallbackContext> CanceledAsObservable( this InputAction inputAction ) =>
			Observable.FromEvent<InputAction.CallbackContext>
			(
				addHandler: action => inputAction.canceled += action,
				removeHandler: action => inputAction.canceled -= action
			);

		public static IDisposable SubscribeToPerformed( this InputAction inputAction, Action<InputAction.CallbackContext> action ) =>
			inputAction
				.PerformedAsObservable()
				.Subscribe( action );

		public static IDisposable SubscribeToCancel( this InputAction inputAction, Action<InputAction.CallbackContext> action ) =>
			inputAction
				.CanceledAsObservable()
				.Subscribe( action );

		
	}
	#endif
}