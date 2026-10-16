using System;
using _Project.Develop.Editing.Photo;
using _Project.Develop.Sound;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Editing.States
{
    public class ChecklistState : IState, IDisposable
    {
        private readonly EditView _editView;

        private Image[] _strikes;
        private EditingChecklist _checklist;
        
        private int _numberToCrossOut;
        
        public ChecklistState(EditView editView) => _editView = editView;

        public void Init()
        {
            _editView.GetStrikes(out _strikes, out _checklist);
        }
        
        public ChecklistState WithNumber(int numberPhoto)
        {
            _numberToCrossOut = numberPhoto - 1;
            return this;
        }
        
        public async UniTask Enter()
        {
            G.Get<EventBus>().ChecklistDown += OnChecklistDown;
            
            _checklist.SimulateHover().Forget();
            _checklist.Enable();
            
            await UniTask.CompletedTask;
        }

        private void OnChecklistDown()
        {
            OnChecklistDownAsync().Forget();
        }

        private async UniTaskVoid OnChecklistDownAsync()
        {
            G.Get<EventBus>().ChecklistDown -= OnChecklistDown;
            _checklist.Disable().Forget();
            
            G.Get<SoundController>().ActionHandle.PlayCross();
            
            var strike = _strikes[_numberToCrossOut];
            
            var t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime * 3f;
                
                var noise = Mathf.PerlinNoise(Time.time * 18f, 0f) * 0.08f;
                
                strike.fillAmount = Mathf.Clamp01(t + noise);
                
                await UniTask.Yield();
            }
            
            G.Get<EditController>().StateController.ChangeState(G.Get<EditController>().EditingState);
        }

        public async UniTask Exit()
        {
            await UniTask.CompletedTask;
        }

        public void Dispose()
        {
            G.Get<EventBus>().ChecklistDown -= OnChecklistDown;
        }
    }
}