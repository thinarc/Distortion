using FMOD.Studio;
using FMODUnity;

namespace _Project.Scripts.Sound.Handles
{
    public class MusicHandle
    {
        private EventInstance _mainTheme;
        private EventInstance _pianoTheme;
        private EventInstance _bgCello;
        
        // Ссылка на текущий играющий трек
        private EventInstance _currentTrack;

        public void Initialize()
        {
            _mainTheme = RuntimeManager.CreateInstance("event:/Music/Main_Them");
            _pianoTheme = RuntimeManager.CreateInstance("event:/Music/S_Piano_Th");
            _bgCello = RuntimeManager.CreateInstance("event:/Music/Bg_Cello");
        }

        public void PlayMainTheme()
        {
            SwitchTo(_mainTheme);
        }

        public void PlayPianoTheme()
        {
            SwitchTo(_pianoTheme);
        }

        public void PlayBgCello()
        {
            SwitchTo(_bgCello);
        }

        // --- НОВЫЙ МЕТОД: ВЫБОР СЛУЧАЙНОГО ТРЕКА ДЛЯ ФОНА ---
        public void PlayRandomBackgroundMusic()
        {
            // Генерируем случайное число от 0 до 2 включительно
            int randomTrack = UnityEngine.Random.Range(0, 3);

            switch (randomTrack)
            {
                case 0:
                    SwitchTo(_mainTheme);
                    break;
                case 1:
                    SwitchTo(_pianoTheme);
                    break;
                case 2:
                    SwitchTo(_bgCello);
                    break;
            }
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
        
        public void SetLowPass(float value)
        {
            if (_currentTrack.isValid())
            {
                _currentTrack.setParameterByName("LowPass", value, false);
            }
        }

        public void SetVolume(float value)
        {
            if (_currentTrack.isValid())
            {
                _currentTrack.setParameterByName("Volume", value, false);
            }
        }
    }
}