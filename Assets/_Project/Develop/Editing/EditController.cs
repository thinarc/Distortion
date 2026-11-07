using System;
using _Project.Develop.Editing.States;
using _Project.Develop.Editing.Tools;

namespace _Project.Develop.Editing
{
    public class EditController : IService, IDisposable
    {
        public StateController StateController { get; private set; }
        
        public EditingState EditingState { get; private set; }
        public RotateState RotateState { get; private set; }
        public DecisionState DecisionState { get; private set; }
        public ChecklistState ChecklistState { get; private set; }

        public int Revision { get; private set; }
        public bool Resetting { get; private set; }
        private bool _fullReset;
        private readonly EditView _editView;

        public EditController(EditView editView)
        {
            _editView = editView;
        }

        public void Init(SprayTool sprayTool)
        {
            EditingState = new EditingState(_editView, sprayTool);
            RotateState = new RotateState(_editView);
            DecisionState = new DecisionState(_editView);
            ChecklistState = new ChecklistState(_editView);

            EditingState.Init();
            DecisionState.Init();
            ChecklistState.Init();
            
            StateController = new StateController();
            StateController.ChangeState(EditingState);
            var bus = G.Get<EventBus>();
            bus.PreSceneReset += StopEditing;
            bus.RunReset += OnRunReset;
            bus.SceneReset += ResetEditing;
            bus.SceneReady += ResumeEditing;
        }

        public void CompletePhoto() => _editView.CompletePhoto();

        private void OnRunReset() => _fullReset = true;

        private void StopEditing()
        {
            Resetting = true;
            Revision++;
            RotateState.Dispose();
            ChecklistState.Dispose();
            _editView.StopEditing();
        }

        private void ResetEditing()
        {
            _editView.ResetEditing(_fullReset);
            _fullReset = false;
        }

        private void ResumeEditing()
        {
            if (!Resetting) return;
            Resetting = false;
            StateController.Reset();
            StateController.ChangeState(EditingState);
        }

        public void Dispose()
        {
            var bus = G.Get<EventBus>();
            bus.PreSceneReset -= StopEditing;
            bus.RunReset -= OnRunReset;
            bus.SceneReset -= ResetEditing;
            bus.SceneReady -= ResumeEditing;
            Revision++;
            EditingState.Dispose();
            RotateState.Dispose();
            ChecklistState.Dispose();
        }
    }
}