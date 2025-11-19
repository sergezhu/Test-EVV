namespace Code.Input.Touch
{
	using UniRx;
	using UnityEngine;

	public interface ITouchInput
	{
		ReactiveCommand<TouchData> OnTouchStart { get; }
		ReactiveCommand<TouchData> OnTouchEnd { get; }
		ReactiveCommand<TouchData> OnTouchPositionChanged { get; }
		Vector2 TouchPosition { get; }
	}
}