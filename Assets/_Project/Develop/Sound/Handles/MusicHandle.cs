using FMOD.Studio;
using FMODUnity;
using System;
using UnityEngine;

namespace _Project.Develop.Sound.Handles
{
    public class MusicHandle : IDisposable
    {
        // ===== FIELDS =====

        private EventInstance _mainTheme;
        private EventInstance _bgMusic;
        private EventInstance _currentTrack;


        // ===== INIT / DISPOSE =====

        public void Init()
        {
            _mainTheme = RuntimeManager.CreateInstance("event:/Music/Main_Them");
            _bgMusic = RuntimeManager.CreateInstance("event:/Music/BG_Music");

            _bgMusic.set3DAttributes(Vector3.zero.To3DAttributes());
        }

        public void Dispose()
        {
            if (_mainTheme.isValid())
            {
                _mainTheme.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _mainTheme.release();
            }

            if (_bgMusic.isValid())
            {
                _bgMusic.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _bgMusic.release();
            }
        }


        // ===== TRACK CONTROL =====

        public void PlayMainTheme()
        {
            SwitchTo(_mainTheme);
        }

        public void PlayMainThemeDirect()
        {
            _mainTheme.start();
        }

        public void StopMainThemeDirect()
        {
            _mainTheme.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        public void PlayBgMusic()
        {
            SwitchTo(_bgMusic);
        }

        public void StopMusic()
        {
            if (_currentTrack.isValid())
            {
                _currentTrack.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }

        public void PauseMusic()
        {
            if (_currentTrack.isValid())
            {
                _currentTrack.setPaused(true);
            }
        }

        public void ResumeMusic()
        {
            if (_currentTrack.isValid())
            {
                _currentTrack.setPaused(false);
            }
        }

        private void SwitchTo(EventInstance newTrack)
        {
            if (_currentTrack.isValid())
            {
                _currentTrack.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }

            _currentTrack = newTrack;
            _currentTrack.start();
        }


        // ===== PARAMETERS =====

        public void SetWindow(float value) => SetParameter("Window", value);
        public void SetVolume(float value) => SetParameter("Volume", value);
        public void SetBeforeRumble(float value) => SetParameter("BeforeRumble", value);
        public void SetEvil(float value) => SetParameter("Evil", value);
        public void SetRumble(float value) => SetParameter("Rumble", value);

        private void SetParameter(string name, float value)
        {
            if (!_currentTrack.isValid())
            {
                Debug.Log($"Set{name}: _currentTrack is NOT valid!");
                return;
            }

            _currentTrack.setParameterByName(name, value, false);
            Debug.Log($"Set{name} called with value: {value}");
        }

        public void SetRadioPosition(Vector3 position)
        {
            if (_bgMusic.isValid())
            {
                _bgMusic.set3DAttributes(position.To3DAttributes());
            }
        }

        public void ResetParameters()
        {
            SetEvil(0f);
            SetRumble(0f);
            SetBeforeRumble(0f);
        }
    }
}