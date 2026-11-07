using System;
using _Project.Develop.Editing.Photo;
using Cysharp.Threading.Tasks;

namespace _Project.Develop.Editing.States
{
    public class ChecklistState : IState, IDisposable
    {
        private readonly EditView _editView;
        
        private EditingChecklist _checklist;
        
        private int _numberToCrossOut;
        
        public ChecklistState(EditView editView) => _editView = editView;

        public void Init()
        {
            _checklist ??= _editView.Checklist;
        }
        
        public ChecklistState WithNumber(int numberPhoto)
        {
            _numberToCrossOut = numberPhoto - 1;
            return this;
        }
        
        public async UniTask Enter()
        {
            G.Get<EventBus>().ChecklistDown += OnChecklistDown;
            
            _checklist.Enable(_numberToCrossOut);
            _editView.ShowChecklistTip();
            
            await UniTask.CompletedTask;
        }

        private static void OnChecklistDown()
        {
            G.Get<EventBus>().ChecklistDown -= OnChecklistDown;
            if (G.Get<EditController>().Resetting) return;
            
            G.Get<EditController>().StateController.ChangeState(G.Get<EditController>().EditingState);
        }

        public async UniTask Exit()
        {
            _editView.HideChecklistTip();
            await UniTask.CompletedTask;
        }

        public void Dispose()
        {
            _editView.HideChecklistTip();
            G.Get<EventBus>().ChecklistDown -= OnChecklistDown;
        }
    }
}
