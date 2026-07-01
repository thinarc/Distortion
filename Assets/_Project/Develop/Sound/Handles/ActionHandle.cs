using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace _Project.Scripts.Sound.Handles
{
    public class ActionHandle
    {
        private EventInstance _openCurtain;
        private EventInstance _brush;
        private EventInstance _nag;
        private EventInstance _spray;
        private EventInstance _photo;
        private EventInstance _cross;
        private float _lastPlayTime;
        private const float StopDelay = 0.15f; // Время сглаживания «песка»
        private bool _isPlayRequested;

        private float _curtainSpeed;

        public void Initialize()
        {
            _openCurtain = RuntimeManager.CreateInstance("event:/SFX/Action/OpenCurtain");
            _brush = RuntimeManager.CreateInstance("event:/SFX/Action/Brush");
            _nag = RuntimeManager.CreateInstance("event:/SFX/Action/Nag");
            _spray = RuntimeManager.CreateInstance("event:/SFX/Action/Spray");
            _photo = RuntimeManager.CreateInstance("event:/SFX/Action/Photo");
            _cross = RuntimeManager.CreateInstance("event:/SFX/Action/Cross");
            _curtainSpeed = 0f;
        }

        public void Tick()
        {
            if (_curtainSpeed > 0) _curtainSpeed -= Time.deltaTime;
            SetCurtainSpeedParameter(_curtainSpeed);
        }

        public void AddCurtainVelocity(float velocity)
        {
            _curtainSpeed = Mathf.Clamp(_curtainSpeed + velocity, 0f, 1f);
        }

        private void SetCurtainSpeedParameter(float speed)
        {
            speed = Mathf.Clamp(speed, 0, 1);
            speed = 1f - speed;
            _openCurtain.setParameterByName("CurtainSpeed", speed);
        }
        
        // Логика звука движения шторы
        public void PlayCurtain()
        {
            _openCurtain.getPlaybackState(out var state);
            
            // Если звук вообще не играет — стартуем его
            if (state == PLAYBACK_STATE.STOPPED)
            {
                _openCurtain.start();
            }
            
            // Запоминаем время последнего движения мыши
            _lastPlayTime = Time.time;

            if (!_isPlayRequested)
            {
                _isPlayRequested = true;
                // Запускаем внутренний таймер затухания, если его еще нет
                StopAfterDelay();
            }
        }

        private async void StopAfterDelay()
        {
            // Пока мышь двигается и время с последнего OnDrag меньше чем StopDelay — ждем
            while (Time.time - _lastPlayTime < StopDelay)
            {
                // Ждем один кадр Unity
                await UniTask.Yield();
            }

            // Время вышло, мышь остановилась — глушим звук шторы
            _openCurtain.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _isPlayRequested = false;
        }

        // Этот метод вызывается из OnPointerUp и на краях шторы для мгновенной остановки
        public void StopCurtainImmediate()
        {
            _lastPlayTime = 0f; // Сбрасываем таймер
            _openCurtain.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        
        // Подключчаю звук кисти и логику
        public void PlayBrush()
        {
            _brush.start();
        }
        // прикрутить звук ластика
        public void PlayNag()
        {
            _nag.start();
        }
        // SPRAY SOUND
        public void PlaySpray()
        {
            _spray.getPlaybackState(out var state);
            
            // Если звук вообще не играет — стартуем его
            if (state == PLAYBACK_STATE.STOPPED)
            {
                _spray.start();
            }
            
        }

        public void StopSpray()
        {
            _spray.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        // Change a photo
        public void PlayPhoto()
        {
            _photo.start();
        }
        // вычеркнул фотку
        public void PlayCross()
        {
            _cross.start();
        }
    }
}