using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace _Project.Develop.Sound.Handles
{
    public class AnomalyHandle : IDisposable
    {
        // ===== FIELDS =====
        
        private bool _playerOnStreet;

        // Инстансы только для зацикленных (Loop) эвентов — one-shot звуки инстансов не требуют
        private EventInstance _mumbling;
        private EventInstance _footsteps;
        private EventInstance _gravestone;

        // Лупы, которые приглушаются, когда игрок не на улице
        private readonly List<EventInstance> _streetMuffledLoops = new();

        
        // ===== INIT / DISPOSE =====
        
        public void Init()
        {
            _mumbling = RuntimeManager.CreateInstance("event:/Anomaly/Mumbling");
            _footsteps = RuntimeManager.CreateInstance("event:/Anomaly/Footsteps");
            _gravestone = RuntimeManager.CreateInstance("event:/Anomaly/Gravestone");
            
            _streetMuffledLoops.Add(_gravestone);
        }

        // Обязательный метод для очистки памяти FMOD (вызывается один раз при закрытии приложения)
        public void Dispose()
        {
            if (_mumbling.isValid())
            {
                _mumbling.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _mumbling.release();
            }

            if (_footsteps.isValid())
            {
                _footsteps.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _footsteps.release();
            }

            if (_gravestone.isValid())
            {
                _gravestone.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _gravestone.release();
            }
        }
        
        public void ResetPlayerOnStreet()
        {
            SetPlayerOnStreet(false);
        }

        
        // ===== UNIVERSAL ENTRY POINTS =====
        // Используются из AnomalyObject через enum AnomalySound — единая точка входа,
        // чтобы вызывающий код не знал, какой конкретно метод дёргать
        
        public void Play(AnomalySound sound)
        {
            Debug.Log($"[AnomalyHandle] Play called with: {sound}");

            switch (sound)
            {
                case AnomalySound.Mumbling:
                    Mumbling();
                    break;
                case AnomalySound.Footsteps:
                    Footsteps();
                    break;
                case AnomalySound.Gravestone:
                    Gravestone();
                    break;
                case AnomalySound.DerBote1:
                    DerBote1();
                    break;
                case AnomalySound.Nerve:
                    Nerve();
                    break;
                case AnomalySound.Clear:
                    Clear();
                    break;
                case AnomalySound.Door:
                    Door();
                    break;
                case AnomalySound.Laugh:
                    Laugh();
                    break;
                case AnomalySound.Flowers:
                    Flowers();
                    break;
                case AnomalySound.BadToy:
                    BadToy();
                    break;
                case AnomalySound.Toy:
                    Toy();
                    break;
                case AnomalySound.None:
                    break;
            }
        }
        
        public void Stop(AnomalySound sound)
        {
            switch (sound)
            {
                case AnomalySound.Mumbling:
                    StopMumbling();
                    break;
                case AnomalySound.Footsteps:
                    GoOut();
                    break;
                case AnomalySound.Gravestone:
                    DigUp();
                    break;
            }
        }

        // Экстренная остановка всех лупов разом — например, на SceneReset
        public void StopAll()
        {
            if (_mumbling.isValid()) _mumbling.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            if (_footsteps.isValid()) _footsteps.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            if (_gravestone.isValid()) _gravestone.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        
        // ===== LOOP SOUNDS (individual control) =====
        
        public void Mumbling()
        {
            if (_mumbling.isValid())
                _mumbling.start();
        }

        public void StopMumbling()
        {
            if (_mumbling.isValid())
                _mumbling.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        public void Footsteps()
        {
            if (_footsteps.isValid())
                _footsteps.start();
        }

        public void GoOut()
        {
            if (_footsteps.isValid())
                _footsteps.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        public void Gravestone()
        {
            if (!_gravestone.isValid()) return;
            ApplyStreetMuffle();
            _gravestone.start();
        }

        public void DigUp()
        {
            if (_gravestone.isValid())
                _gravestone.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        
        // ===== ONE-SHOT SOUNDS =====
        
        public void DerBote1() => RuntimeManager.PlayOneShot("event:/Anomaly/OneShot/DerBote1");
        public void Nerve() => RuntimeManager.PlayOneShot("event:/Anomaly/OneShot/Nerve");
        public void Clear() => RuntimeManager.PlayOneShot("event:/Anomaly/OneShot/Clear");
        public void Door() => RuntimeManager.PlayOneShot("event:/Anomaly/OneShot/Door");
        public void Laugh() => RuntimeManager.PlayOneShot("event:/Anomaly/OneShot/Sinister");
        public void Flowers() => RuntimeManager.PlayOneShot("event:/Anomaly/OneShot/Flowers");
        public void BadToy() => RuntimeManager.PlayOneShot("event:/Anomaly/OneShot/BadToy");
        public void Toy() => RuntimeManager.PlayOneShot("event:/Anomaly/OneShot/Toy");

        
        // ===== STREET MUFFLE =====
        // Приглушение уличных лупов, когда игрок физически не на улице
        
        public void SetPlayerOnStreet(bool onStreet)
        {
            _playerOnStreet = onStreet;
            ApplyStreetMuffle();
        }

        public void ApplyStreetMuffle()
        {
            var muffled = !_playerOnStreet;

            foreach (var streetMuffledLoop in _streetMuffledLoops)
            {
                if (!streetMuffledLoop.isValid()) continue;
                streetMuffledLoop.setParameterByName("Window", muffled ? 0.8f : 0f);
                //streetMuffledLoop.setParameterByName("Volume", muffled ? 0.3f : 1f);
            }
        }

        
        // ===== PARAMETERS =====
        
        public void SetWindow(float value)
        {
            if (_footsteps.isValid())
            {
                _footsteps.setParameterByName("Window", value, false);
            }
        }
    }

    public enum AnomalySound
    {
        None = 0,
        Mumbling = 1,
        Footsteps = 2,
        DerBote1 = 3,
        Nerve = 4,
        Clear = 5,
        Door = 6,
        Laugh = 7,
        Flowers = 8,
        Gravestone = 9,
        BadToy = 10,
        Toy = 11
    }
}