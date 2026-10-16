using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using _Project.Develop.Sound;
using _Project.Develop.UI.Menus;

namespace _Project.Develop.Switch.Scenes
{
    public class BedLocation : BaseLocation
    {
        [SerializeField, Space(5)] private ParticleSystem dust;
        [SerializeField] private Button bedButton;

        [SerializeField, Space(5)] private float restMultiplier = 2f;
        
        private bool _showing;
        
        public override async UniTask Show()
        {
            if (CurrentAnomalies.Count == 0)
            {
                _ = Tween.Custom(
                    startValue: 4f,
                    endValue: 0.7f,
                    duration: 2.7f,
                    onValueChange: value =>
                    {
                        var main = dust.main;
                        main.simulationSpeed = value;
                    }).OnComplete(() => bedButton.interactable = CurrentAnomalies.Count == 0);
            }
            else bedButton.interactable = false;
            
            var task = base.Show();
            dust.Play();
            
            await task;
            
            G.Get<SoundController>().MusicHandle.SetRadioPosition(new Vector3(0f, 0f, -11f));
            G.Get<SoundController>().MusicHandle.SetWindow(0.43f);
            G.Get<SoundController>().MusicHandle.SetVolume(1f);
            
            while (_showing)
            {
                await UniTask.Delay(100);

                if (CurrentAnomalies.Count == 0) continue;
                dust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                bedButton.interactable = false;
            }
        }

        public override async UniTask Hide(float duration, Ease ease)
        {
            var task = base.Hide(duration, ease);

            await UniTask.Delay(100);
            dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); 
            
            await task;
            
            _showing = false;
            
            G.Get<SoundController>().MusicHandle.SetWindow(0f);
            G.Get<SoundController>().MusicHandle.SetVolume(0f);
        }
        
        public void CallRest()
        {
            G.Get<RestDialog>().ApplyRest(restMultiplier, RestDialog.RestType.Sleep).Forget();
            
            dust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            bedButton.interactable = false;
        }
    }
}