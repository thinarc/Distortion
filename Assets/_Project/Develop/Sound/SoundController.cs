using _Project.Develop.Sound.Handles;
using Cysharp.Threading.Tasks;
using FMOD.Studio;
using UnityEngine;

namespace _Project.Develop.Sound
{
    public class SoundController : IService
    {
        private float _masterVolume;
        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                if (value is > 1f or < 0) return;
                _masterVolume = value;
                _masterBus.setVolume(_masterVolume);
                PlayerPrefs.SetFloat("Master", _masterVolume);
            }
        }
        
        private Bus _masterBus;
        private Bus _pauseBus;
        
        public MusicHandle MusicHandle { get; private set; } = new();
        public SFXHandle SfxHandle { get; private set; } = new();
        public AmbientHandle AmbientHandle { get; private set; } = new();
        public UIHandle UIHandle { get; private set; } = new();
        public ActionHandle ActionHandle { get; private set; } = new();
        public AnomalyHandle AnomalyHandle { get; private set; } = new();
        
        public void Initialize()
        { 
            _masterBus = FMODUnity.RuntimeManager.GetBus("bus:/");
            _pauseBus = FMODUnity.RuntimeManager.GetBus("bus:/SetPause");
            
            MasterVolume = PlayerPrefs.GetFloat("Master", 0.6f);
            
            MusicHandle.Initialize();
            // SfxHandle.Initialize();
            AmbientHandle.Initialize();
            UIHandle.Initialize();
            // ActionHandle.Initialize();
            // AnomalyHandle.Initialize();
            
            UpdateCycle();
        }

        public void EnterPause()
        {
            _pauseBus.setPaused(true);
            MusicHandle.PlayMainThemeDirect();
        }

        public void ExitPause()
        {
            MusicHandle.StopMainThemeDirect();
            _pauseBus.setPaused(false);
        }
        
        private async void UpdateCycle()
        {
            while (true)
            {
                ActionHandle.Tick();
                await UniTask.Yield();
            }
        }
    }
}