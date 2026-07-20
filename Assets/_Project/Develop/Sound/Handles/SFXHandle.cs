// connect library
using FMOD.Studio;
using FMODUnity;

// address 
namespace _Project.Develop.Sound.Handles
{
    // class named
    public class SFXHandle
        
        
        // set variables
    {
        private EventInstance _caution;
        private EventInstance _lightout;
       

        public void Initialize()
            // create a live copy for event to control it
        { 
            _caution = RuntimeManager.CreateInstance("event:/SFX/Event/Caution");
                //_caution.set3DAttributes(RuntimeUtils.To3DAttributes(Vector3.zero));
            _lightout = RuntimeManager.CreateInstance("event:/SFX/Event/LightOut");
            
                
        }

        
        // external buttons
        //Just like play&stop
        
        
        // CAUTION
        public void PlayCaution()
        {
            _caution.start();
        }

        public void StopCaution()
        {
            _caution.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        
        // LIGHT OUT - звук перегорания лампы
        public void PlayLightOut()
        {
            _lightout.start();
        }

        public void StopLightOut()
        {
            _lightout.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        
        
         
        
        // im setting all parametrs here
        //public void SetLowPass(float value)
       // {
         //   _currentAmbient.setParameterByName("LowPass", value);
        //}
        
    }
    
    
}