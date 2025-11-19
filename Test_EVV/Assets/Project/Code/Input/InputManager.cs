namespace Code.Input
{
    using Code.Core;

    public interface IInputManager
    {
        InputActions.TouchActions Touch { get; }
    }
    


    public class InputManager : IInputManager, IInitializable
    {
        private InputActions _actions;

        public InputActions.TouchActions Touch { get; private set; }
        
        public void Initialize()
        {
            _actions = new InputActions();
            
            Touch = _actions.Touch;
        }
    }
}