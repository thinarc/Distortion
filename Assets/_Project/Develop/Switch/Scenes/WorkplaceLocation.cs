using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using _Project.Develop.Sound;

namespace _Project.Develop.Switch.Scenes
{
    public class WorkplaceLocation : BaseLocation
    {
        [SerializeField, Space(5)] private Image thingsButton;
        [SerializeField] private CanvasGroup stats;
        
        public override async UniTask Show()
        {
            _ = Tween.Alpha(thingsButton, endValue: 1f, duration: TransitionTime, ease: TransitionEase);
            thingsButton.raycastTarget = true;
            
            var task = base.Show();
            
            _ = Tween.Alpha(stats, endValue: 1f, duration: TransitionTime, ease: TransitionEase);
            stats.blocksRaycasts = true;
            
            await task;
            
            G.Get<SoundController>().MusicHandle.SetRadioPosition(new Vector3(0f, 0f, -9f));
        }
        
        public override async UniTask Hide(float duration, Ease ease)
        {
            _ = Tween.Alpha(thingsButton, endValue: 0f, duration, ease);
            thingsButton.raycastTarget = false;
            
            _ = Tween.Alpha(stats, endValue: 0f, duration, ease);
            stats.blocksRaycasts = false;
            
            await base.Hide(duration, ease);
        }
    }
}