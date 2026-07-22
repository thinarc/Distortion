using FMOD.Studio;
using FMODUnity;

namespace _Project.Develop.Sound.Handles
{
    public class MusicHandle
    {
        private EventInstance _mainTheme;
        private EventInstance _bgMusic;
        
        
        // Ссылка на текущий играющий трек
        private EventInstance _currentTrack;

        public void Initialize()
        {
            _mainTheme = RuntimeManager.CreateInstance("event:/Music/Main_Them");
            _bgMusic = RuntimeManager.CreateInstance("event:/Music/BG_Music");
            
        }

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
        // == PARAMETRS ==
        public void SetWindow(float value)
        {
            if (_currentTrack.isValid())
            {
                _currentTrack.setParameterByName("Window", value, false);
            }
        }

        public void SetBeforeRumble(float value)
        {
            if (_currentTrack.isValid())
            {
                _currentTrack.setParameterByName("BeforeRumble", value, false);
            }
        }
        
        public void SetEvil(float value)
        {
            if (_currentTrack.isValid())
            {
                _currentTrack.setParameterByName("Evil", value, false);
            }
        }
        
        public void SetRumble(float value)
        {
            if (_currentTrack.isValid())
            {
                _currentTrack.setParameterByName("Rumble", value, false);
            }
        }
    }
}