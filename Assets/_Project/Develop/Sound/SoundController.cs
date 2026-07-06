using _Project.Scripts.Sound.Handles;
using Cysharp.Threading.Tasks;
using FMOD.Studio;
using UnityEngine;

namespace _Project.Develop.Sound
{
    public class SoundController : IService
    {
        // Master
        
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
        
        // Handles
        
        public MusicHandle MusicHandle { get; private set; } = new();
        public SFXHandle SfxHandle { get; private set; } = new();
        public AmbientHandle AmbientHandle { get; private set; } = new();
        public UIHandle UIHandle { get; private set; } = new();
        public ActionHandle ActionHandle { get; private set; } = new();
        public AnomalyHandle AnomalyHandle { get; private set; } = new();
        
        public void Initialize()
        {
            // Master
            // _masterBus = FMODUnity.RuntimeManager.GetBus("bus:/");
            
            MasterVolume = PlayerPrefs.GetFloat("Master", 0.6f);
            
            // I set ALL HANDLE here
            // MusicHandle.Initialize();
            // SfxHandle.Initialize();
            // AmbientHandle.Initialize();
            // UIHandle.Initialize();
            // ActionHandle.Initialize();
            // AnomalyHandle.Initialize();
            
            // I set background music there
            //MusicHandle.PlayPianoTheme();
            
            // сбросить на нормальные значения после инициализации
            // MusicHandle.SetLowPass(1f);
            // MusicHandle.SetVolume(1f);
            
            UpdateCycle();
        }
        
        private async void UpdateCycle()
        {
            while (true)
            {
                ActionHandle.Tick();
                await UniTask.NextFrame();
            }
        }
    }
}