using System;
using _Project.Develop.Sound.Handles;
using Cysharp.Threading.Tasks;
using FMOD.Studio;
using PrimeTween;
using UnityEngine;

namespace _Project.Develop.Sound
{
    public class SoundController : IService, IDisposable
    {
        // ===== MASTER VOLUME =====

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


        // ===== FIELDS =====

        private Bus _masterBus;
        private Bus _pauseBus;
        private Tween _restTransition;
        private int _restRevision;
        
        // Флаг для блокировки музыки во время концовки
        private bool _isEndingSequence;

        public MusicHandle MusicHandle { get; } = new();
        public SFXHandle SfxHandle { get; } = new();
        public AmbientHandle AmbientHandle { get; } = new();
        public UIHandle UIHandle { get; } = new();
        public ActionHandle ActionHandle { get; } = new();
        public AnomalyHandle AnomalyHandle { get; } = new();
        public EndingHandle EndingHandle { get; } = new();


        // ===== INIT / DISPOSE =====

        public void Init()
        {
            _masterBus = FMODUnity.RuntimeManager.GetBus("bus:/");
            _pauseBus = FMODUnity.RuntimeManager.GetBus("bus:/SetPause");

            MasterVolume = PlayerPrefs.GetFloat("Master", 0.6f);

            MusicHandle.Init();
            SfxHandle.Init();
            AmbientHandle.Init();
            UIHandle.Init();
            ActionHandle.Init();
            AnomalyHandle.Init();
            EndingHandle.Init();

            UpdateCycle().Forget();

            G.Get<EventBus>().SceneReset += OnSceneReset;
            G.Get<EventBus>().SceneReady += OnSceneReady;
        }

        public void Dispose()
        {
            MusicHandle.Dispose();
            SfxHandle.Dispose();
            AmbientHandle.Dispose();
            UIHandle.Dispose();
            ActionHandle.Dispose();
            AnomalyHandle.Dispose();

            G.Get<EventBus>().SceneReset -= OnSceneReset;
            G.Get<EventBus>().SceneReady -= OnSceneReady;
        }


        //  МЕТОД ДЛЯ УПРАВЛЕНИЯ СОСТОЯНИЕМ КОНЦОВКИ
        
        public void SetEndingState(bool isEnding)
        {
            _isEndingSequence = isEnding;
            
            if (isEnding)
            {
                MusicHandle.StopMusic();
            }
        }


        // ===== PAUSE (bus-level, покрывает всё разом) =====

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

        public async UniTask PauseForRest()
        {
            var revision = _restRevision;
            _restTransition = Tween.Custom(1f, 0f, 1f, onValueChange: v => _pauseBus.setVolume(v));
            await _restTransition;
            if (revision == _restRevision) _pauseBus.setPaused(true);
        }

        public async UniTask ResumeForRest()
        {
            _pauseBus.setPaused(false);
            _restTransition.Stop();
            _restTransition = Tween.Custom(0f, 1f, 1f, onValueChange: v => _pauseBus.setVolume(v));
            await _restTransition;
        }


        // ===== SCENE LIFECYCLE (занавес вниз / занавес вверх) =====

        private void OnSceneReset()
        {
            _restRevision++;
            _restTransition.Stop();
            _pauseBus.setPaused(false);
            _pauseBus.setVolume(1f);
            ActionHandle.StopSpray();
            ActionHandle.StopCurtainImmediate();
            Debug.Log("[SoundController] SceneReset — останавливаю звук");

            MusicHandle.StopMusic();
            AmbientHandle.StopStreet();
            AmbientHandle.StopAmbience();
            AnomalyHandle.StopAll();

            AmbientHandle.ResetStreetState();
            AnomalyHandle.ResetPlayerOnStreet();
            MusicHandle.ResetParameters();
        }

        private void OnSceneReady()
        {
            Debug.Log("[SoundController] SceneReady — новый акт");

            // Если идет концовка, музыку НЕ включаем!
            Debug.Log($"[SoundController] SceneReady вызван. Флаг концовки: {_isEndingSequence}");

            if (_isEndingSequence)
            {
                Debug.Log("[SoundController] ❌ Музыка не запущена из-за концовки");
                return;
            }
    
            Debug.Log("[SoundController] ✅ Запускаю музыку!");
            
            MusicHandle.PlayBgMusic();
            AmbientHandle.PlayAmbience();
        }


        // ===== TICK =====

        private async UniTaskVoid UpdateCycle()
        {
            while (true)
            {
                ActionHandle.Tick();
                await UniTask.Yield();
            }
        }
    }
}