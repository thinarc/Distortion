using System;
using _Project.Develop.Editing.Photo;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

namespace _Project.Develop.Editing.States
{
    public class RotateState : IEditState, IDisposable
    {
        private readonly EditView _editView;
        
        private EditingPhoto _photoToRotate;

        public RotateState(EditView editView) => _editView = editView;

        public RotateState WithPhoto(EditingPhoto photoToRotate)
        {
            _photoToRotate = photoToRotate;
            return this;
        }
        
        public async UniTask Enter()
        {
            G.Get<EventBus>().PhotoDown += OnPhotoDown;
            
            await UniTask.CompletedTask;
        }

        private void OnPhotoDown() => G.Get<EditController>().StateController.ChangeState(G.Get<EditController>().DecisionState.WithPhoto(_photoToRotate));

        public async UniTask Exit()
        {
            G.Get<EventBus>().PhotoDown -= OnPhotoDown;
            
            _ = Tween.Alpha(_editView.SpotsGroup, endValue: 0, duration: 0.2f, ease: Ease.InQuad);

            await Tween.Rotation(_photoToRotate.transform, endValue: new Vector3(0f, 90f, 0f), duration: 0.15f, ease: Ease.InQuad);
            _photoToRotate.Image.sprite = _photoToRotate.BackdropPhoto;

            await Tween.Rotation(_photoToRotate.transform, endValue: new Vector3(0f, 180f, 0f), duration: 0.25f, ease: Ease.OutQuad);
        }

        public void Dispose()
        {
            G.Get<EventBus>().PhotoDown -= OnPhotoDown;
        }
    }
}