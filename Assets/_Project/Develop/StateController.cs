namespace _Project.Develop
{
    public class StateController
    {
        private IState _currentState;

        public void ChangeState(IState state)
        {
            _currentState?.Exit();

            _currentState = state;

            _currentState.Enter();
        }
    }
}