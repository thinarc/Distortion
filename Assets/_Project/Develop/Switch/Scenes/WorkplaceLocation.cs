using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Switch.Scenes
{
    public class WorkplaceLocation : BaseLocation
    {
        [SerializeField, Space(5)] private Image thingsButton;
        
        public override async UniTask Show()
        {
            _ = Tween.Alpha(thingsButton, endValue: 1f, duration: TransitionTime, ease: TransitionEase);
            thingsButton.raycastTarget = true;
            
            await base.Show();
        }
        
        public override async UniTask Hide(float duration, Ease ease)
        {
            _ = Tween.Alpha(thingsButton, endValue: 0f, duration: duration, ease: ease);
            thingsButton.raycastTarget = false;
            
            await base.Hide(duration, ease);
        }
    }
}