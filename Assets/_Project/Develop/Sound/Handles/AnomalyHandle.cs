// connect library
using FMOD.Studio;
using FMODUnity;

// address 
namespace _Project.Scripts.Sound.Handles
{
    // class named
    public class AnomalyHandle
        
        
    // set variables
    {
        private EventInstance _whispered;
        private EventInstance _phone;
        private EventInstance _footsteps;
        private EventInstance _scream;
        
        
        // save the link what is current track
        private EventInstance _currentTrack;

        public void Initialize()
        // create a live copy for event to control it
        {
            _whispered = RuntimeManager.CreateInstance("event:/Anomaly/Whispered");
            _phone = RuntimeManager.CreateInstance("event:/Anomaly/Phone");
            _footsteps = RuntimeManager.CreateInstance("event:/Anomaly/Footsteps");
            _scream = RuntimeManager.CreateInstance("event:/Anomaly/Scream");
        }

        public void Whispered()
        {
            _whispered.start();
        }

        public void StopMumbling()
        {
            _whispered.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        public void Phone()
        {
            _phone.start();
        }

        public void Footsteps()
        {
            _footsteps.start();
        }

        public void GoOut()
        {
            _footsteps.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); 
        }

        public void SetLowPass(float value)
        {
            if (_footsteps.isValid())
            {
                _footsteps.setParameterByName("LowPass", value, false); 
            }
            

        }

        public void Scream()
        {
            _scream.start();
        }


    }
    
    
}