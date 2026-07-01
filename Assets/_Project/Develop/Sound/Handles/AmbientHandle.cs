// connect library
using FMOD.Studio;
using FMODUnity;

// address 
namespace _Project.Scripts.Sound.Handles
{
    // class named
    public class AmbientHandle
        
        
    // set variables
    {
        private EventInstance _deaf;
        private EventInstance _rumble;
        private EventInstance _street;
        private EventInstance _clock;
        
        
        // save the link what is current track
        private EventInstance _currentTrack;

        public void Initialize()
        // create a live copy for event to control it
        {
            _deaf = RuntimeManager.CreateInstance("event:/bg/Deaf");
            _rumble = RuntimeManager.CreateInstance("event:/bg/Rumble");
            _street = RuntimeManager.CreateInstance("event:/bg/Street");
            _clock = RuntimeManager.CreateInstance("event:/bg/Clock");
        }

        
        // external buttons
        public void PlayRumble()
        {
            SwitchTo(_rumble);
        }
        //Deaf just like play&stop
        public void PlayDeaf()
        {
            _deaf.start();
        }

        public void StopDeaf()
        {
            _deaf.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        
        // Street Play&Stop
        public void PlayStreet()
        {
            SwitchTo(_street);
        }

        public void StopStreet()
        {
            _street.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }


        //Lets stop the music
        public void StopAmbient()
        {
            _currentTrack.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _deaf.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        public void PauseAmbient()
        {
            _currentTrack.setPaused(true);
        }

        public void ResumeAmbient()
        {
            _currentTrack.setPaused(false);
        }

        private void SwitchTo(EventInstance newTrack)
        // check: does a current track exist?
        {
            if (_currentTrack.isValid())
                _currentTrack.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _currentTrack = newTrack;
            _currentTrack.start();
        }

        public void GlobalClock()
        {
            _clock.start();
        }

        public void Timeless()
        {
            _clock.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        
        // im setting all parametrs here
        public void SetLowPass(float value)
        {
            _currentTrack.setParameterByName("LowPass", value, false);
        }
        public void SetVolume(float value)
        {
            _currentTrack.setParameterByName("Volume", value,  false);
        }

        public void Nervous(float value)
        {
            _clock.setParameterByName("Nervous", value, false);
        }
        
    }
    
    
}