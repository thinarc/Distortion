// connect library
using FMOD.Studio;
using FMODUnity;

// address 
namespace _Project.Develop.Sound.Handles
{
    // class named
    public class AmbientHandle
        
        
    // set variables
    {
        private EventInstance _street;
        private EventInstance _clock;
        
        
        // save the link what is current track
        private EventInstance _currentTrack;

        public void Initialize()
        // create a live copy for event to control it
        {
            _street = RuntimeManager.CreateInstance("event:/bg/Street");
            _clock = RuntimeManager.CreateInstance("event:/bg/Clock");
        }
        
        // Street Play&Stop
        public void PlayStreet()
        {
            _street.start();
        }

        public void StopStreet()
        {
            _street.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        public void GlobalClock()
        {
            _clock.start();
        }

        public void Timeless()
        {
            _clock.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        
    }
    
    
}