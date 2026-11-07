using System;
using _Project.Develop.Editing.Photo;
using _Project.Develop.Sound;
using _Project.Develop.UI;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

namespace _Project.Develop.Editing.States
{
    public class RotateState : IState, IDisposable
    {
        private readonly EditView _editView;
        
        private EditingPhoto _photoToRotate;
        private UIEffects _effects;

        public RotateState(EditView editView) => _editView = editView;

        public RotateState WithPhoto(EditingPhoto photoToRotate)
        {
            _photoToRotate = photoToRotate;
            _effects ??= photoToRotate.GetComponentInParent<UIEffects>();
            return this;
        }
        
        public async UniTask Enter()
        {
            var controller = G.Get<EditController>();
            var revision = controller.Revision;
            G.Get<EventBus>().PhotoDown += OnPhotoDown;
            _editView.ShowPhotoTip();
            await UniTask.CompletedTask;
            if (revision != controller.Revision) return;
        }

        private void OnPhotoDown() => G.Get<EditController>().StateController.ChangeState(G.Get<EditController>().DecisionState.WithPhoto(_photoToRotate));

        public async UniTask Exit()
        {
            var controller = G.Get<EditController>();
            var revision = controller.Revision;
            G.Get<EventBus>().PhotoDown -= OnPhotoDown;
            _editView.HidePhotoTip();
            G.Get<EventBus>().EndEditing?.Invoke();
            
            G.Get<SoundController>().ActionHandle.PlayPhoto();
            
            _effects.SetEffect(UIEffectType.PressScale, true);
            _effects.SetEffect(UIEffectType.HoverMove, false);
            
            _effects.enabled = true;
            
            await Tween.Rotation(_effects.transform, endValue: new Vector3(0f, 90f, 0f), duration: 0.15f, ease: Ease.InQuad);
            if (revision != controller.Revision) return;
            
            _photoToRotate.Image.sprite = _photoToRotate.BackdropPhoto;
            _editView.SpotsGroup.alpha = 0;

            await Tween.Rotation(_effects.transform, endValue: new Vector3(0f, 180f, 0f), duration: 0.25f, ease: Ease.OutQuad);
            if (revision != controller.Revision) return;
        }

        public void Dispose()
        {
            _editView.HidePhotoTip();
            G.Get<EventBus>().PhotoDown -= OnPhotoDown;
        }
    }
}
