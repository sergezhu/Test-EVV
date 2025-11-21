namespace Code.Input
{
	using Code.Core;

	public interface IInputManager
	{
		InputActions.TouchActions Touch { get; }
	}


	public class InputManager : IInputManager, IInitializable
	{
		private InputActions actions;

		public void Initialize()
		{
			actions = new InputActions();

			Touch = actions.Touch;
			
			Touch.Enable();
		}

		public InputActions.TouchActions Touch { get; private set; }
	}
}