// connect library
using FMOD.Studio;
using FMODUnity;

// address 
namespace _Project.Develop.Sound.Handles
{
    // class named
    public class AnomalyHandle
        
        
    // set variables
    {
        private EventInstance _mumbling;
        private EventInstance _footsteps;
        
        
        // save the link what is current track
        private EventInstance _currentTrack;

        public void Initialize()
        // create a live copy for event to control it
        {
            _mumbling = RuntimeManager.CreateInstance("event:/Anomaly/Mumbling");
            _footsteps = RuntimeManager.CreateInstance("event:/Anomaly/Footsteps");
        }

        public void Mumbling()
        {
            _mumbling.start();
        }

        public void StopMumbling()
        {
            _mumbling.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        
        public void Footsteps()
        {
            _footsteps.start();
        }

        public void GoOut()
        {
            _footsteps.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); 
        }

        public void SetWindow(float value)
        {
            if (_footsteps.isValid())
            {
                _footsteps.setParameterByName("Window", value, false); 
            }
            
        }


    }
    
    
}