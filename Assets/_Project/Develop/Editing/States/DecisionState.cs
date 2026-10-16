using _Project.Develop.Editing.Photo;
using _Project.Develop.UI;
using _Project.Develop.Sound;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

namespace _Project.Develop.Editing.States
{
    public class DecisionState : IState
    {
        private readonly EditView _editView;

        private EditingPhoto _photoToDecision;
        private UIEffects _effects;

        private bool _firstEnter;
        
        public DecisionState(EditView editView) => _editView = editView;

        public void Init()
        {
            _editView.BindButtons(() => OnAcceptPhoto().Forget(), 
                () => OnRejectPhoto().Forget());
        }

        public DecisionState WithPhoto(EditingPhoto photoToDecision)
        {
            _photoToDecision = photoToDecision;
            _effects ??= photoToDecision.GetComponentInParent<UIEffects>();
            return this;
        }
        
        public async UniTask Enter()
        {
            _effects.SetEffect(UIEffectType.PressScale, false);
            _effects.SetEffect(UIEffectType.HoverMove, true);
            
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
            
            _effects.SetEffect(UIEffectType.HoverScale, false);
            _effects.enabled = false;
        }
        
        private async UniTaskVoid OnAcceptPhoto()
        {
            G.Get<SoundController>().UIHandle.PlayClick();
            
            OnChoosePhoto();
            
            _ = Tween.Rotation(_effects.transform, endValue: new Vector3(0f, 0f, 15f), duration: 0.15f);
            
            var effectsRect = _effects.GetComponent<RectTransform>();
            var anchorPos = new Vector2(effectsRect.anchoredPosition.x + 45, effectsRect.anchoredPosition.y);
            
            await Tween.UIAnchoredPosition(effectsRect, anchorPos, duration: 0.9f, ease: Ease.InBack);
            await Tween.Alpha(_photoToDecision.Image, endValue: 0f, duration: 0.3f, ease: Ease.InBack);
            
            OnEndAnimate();
            
            if (_photoToDecision.Ano == null) G.Get<EditController>().StateController.ChangeState(G.Get<EditController>().ChecklistState.WithNumber(_photoToDecision.Number));
            else
            {
                _photoToDecision.Ano.PlaySound(look: true);
                await _photoToDecision.Ano.ActivateSourceTask();
                
                _photoToDecision.Ano.StopSound(look: true);
                
                G.Get<EditController>().StateController.ChangeState(G.Get<EditController>().EditingState);
            }
            
        }

        private async UniTaskVoid OnRejectPhoto()
        {
            G.Get<SoundController>().UIHandle.PlayExit();
            OnChoosePhoto();
            
            var direction = Random.value > 0.5f ? -1f : 1f;

            _ = Tween.Rotation(_effects.transform, endValue: new Vector3(0f, 0f, direction * 10f), duration: 0.2f, ease: Ease.OutQuad);
            await Tween.Alpha(_photoToDecision.Image, endValue: 0f, duration: 0.3f, ease: Ease.OutQuad);
            
            OnEndAnimate();
            
            if (_photoToDecision.Ano == null) _editView.ReturnPhoto(_photoToDecision);
            
            G.Get<EditController>().StateController.ChangeState(G.Get<EditController>().EditingState);
        }

        private void OnChoosePhoto()
        {
            _editView.ButtonsGroup.interactable = false;
            _editView.ButtonsGroup.blocksRaycasts = false;
            
            _effects.enabled = true;
            
            _ = Tween.Alpha(_editView.ButtonsGroup, endValue: 0f, duration: 0.2f, ease: Ease.InBack);
        }

        private void OnEndAnimate()
        {
            _photoToDecision.gameObject.SetActive(false);
        }

        public async UniTask Exit()
        {
            await UniTask.CompletedTask;
        }
    }
}