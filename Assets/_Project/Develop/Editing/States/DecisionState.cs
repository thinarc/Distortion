using _Project.Develop.Editing.Photo;
using _Project.Develop.UI;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

namespace _Project.Develop.Editing.States
{
    public class DecisionState : IEditState
    {
        private readonly EditView _editView;

        private EditingPhoto _photoToDecision;
        private UIEffects _effects;

        private bool _firstEnter;
        
        public DecisionState(EditView editView) => _editView = editView;

        public void Initialize()
        {
            _editView.BindButtons(OnAcceptPhoto, OnRejectPhoto);
        }

        public DecisionState WithPhoto(EditingPhoto photoToDecision)
        {
            _photoToDecision = photoToDecision;
            _effects ??= photoToDecision.GetComponentInParent<UIEffects>();
            return this;
        }
        
        public async UniTask Enter()
        {
            if (!_firstEnter) await UniTask.Delay(400);
            else
            {
                await UniTask.Delay(800);
                _firstEnter = true;
            }
            
            _ = Tween.Alpha(_editView.ButtonsGroup, endValue: 1f, duration: 0.4f, ease: Ease.InBack);
            await UniTask.Delay(200);
            
            _editView.ButtonsGroup.interactable = true;
            _editView.ButtonsGroup.blocksRaycasts = true;
        }
        
        private async void OnAcceptPhoto()
        {
            OnChoosePhoto();
            
            _ = Tween.Rotation(_photoToDecision.transform, endValue: new Vector3(0f, 0f, 15f), duration: 0.15f);

            var anchorPos = new Vector2(_photoToDecision.Image.rectTransform.anchoredPosition.x + 45, _photoToDecision.Image.rectTransform.anchoredPosition.y);
            
            await Tween.UIAnchoredPosition(_photoToDecision.Image.rectTransform, anchorPos, duration: 0.9f, ease: Ease.InBack);
            await Tween.Alpha(_photoToDecision.Image, endValue: 0f, duration: 0.3f, ease: Ease.InBack);
            
            OnEndAnimate();

            if (!_photoToDecision.IsOdd) G.Get<EditController>().StateController.ChangeState(G.Get<EditController>().ChecklistState.WithNumber(_photoToDecision.Number));
            else
            {
                // When make the wrong choice
            }
        }

        private async void OnRejectPhoto()
        {
            OnChoosePhoto();
            
            var direction = Random.value > 0.5f ? -1f : 1f;

            _ = Tween.Rotation(_photoToDecision.transform, endValue: new Vector3(0f, 0f, direction * 10f), duration: 0.2f, ease: Ease.OutQuad);
            await Tween.Alpha(_photoToDecision.Image, endValue: 0f, duration: 0.3f, ease: Ease.OutQuad);
            
            OnEndAnimate();
            
            if (!_photoToDecision.IsOdd) _editView.ReturnPhoto(_photoToDecision);
            
            G.Get<EditController>().StateController.ChangeState(G.Get<EditController>().EditingState);
        }

        private void OnChoosePhoto()
        {
            _editView.ButtonsGroup.interactable = false;
            _editView.ButtonsGroup.blocksRaycasts = false;
            
            _ = Tween.Alpha(_editView.ButtonsGroup, endValue: 0f, duration: 0.2f, ease: Ease.InBack);
        }

        private void OnEndAnimate()
        {
            _photoToDecision.gameObject.SetActive(false);
            _effects.enabled = true;
        }

        public async UniTask Exit()
        {
            await UniTask.CompletedTask;
        }
    }
}