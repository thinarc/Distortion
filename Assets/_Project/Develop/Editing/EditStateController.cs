namespace _Project.Develop.Editing
{
    public class EditStateController
    {
        private IEditState _currentState;

        public void ChangeState(IEditState state)
        {
            _currentState?.Exit();

            _currentState = state;

            _currentState.Enter();
        }
    }
}