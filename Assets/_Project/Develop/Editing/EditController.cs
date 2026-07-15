using System;
using _Project.Develop.Editing.States;
using _Project.Develop.Editing.Tools;

namespace _Project.Develop.Editing
{
    public class EditController : IService, IDisposable
    {
        public EditStateController StateController { get; private set; }
        
        public EditingState EditingState { get; private set; }
        public RotateState RotateState { get; private set; }
        public DecisionState DecisionState { get; private set; }
        public ChecklistState ChecklistState { get; private set; }

        private readonly EditView _editView;

        public EditController(EditView editView)
        {
            _editView = editView;
        }

        public void Initialize(SprayTool sprayTool)
        {
            EditingState = new EditingState(_editView, sprayTool);
            RotateState = new RotateState(_editView);
            DecisionState = new DecisionState(_editView);
            ChecklistState = new ChecklistState(_editView);

            EditingState.Initialize();
            DecisionState.Initialize();
            ChecklistState.Initialize();
            
            StateController = new EditStateController();
            StateController.ChangeState(EditingState);
        }

        public void Dispose()
        {
            EditingState.Dispose();
            RotateState.Dispose();
            ChecklistState.Dispose();
        }
    }
}