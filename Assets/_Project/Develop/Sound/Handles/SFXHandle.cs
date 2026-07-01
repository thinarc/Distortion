// connect library
using FMOD.Studio;
using FMODUnity;

// address 
namespace _Project.Scripts.Sound.Handles
{
    // class named
    public class SFXHandle
        
        
        // set variables
    {
        private EventInstance _caution;
        private EventInstance _regret;
        private EventInstance _ominous;
        private EventInstance _lightout;
       

        public void Initialize()
            // create a live copy for event to control it
        { 
            _caution = RuntimeManager.CreateInstance("event:/SFX/Event/Caution");
                //_caution.set3DAttributes(RuntimeUtils.To3DAttributes(Vector3.zero));
    
            _regret = RuntimeManager.CreateInstance("event:/SFX/Event/Regret");
                //_regret.set3DAttributes(RuntimeUtils.To3DAttributes(Vector3.zero));
    
            _ominous = RuntimeManager.CreateInstance("event:/SFX/Event/Ominous");
                //_ominous.set3DAttributes(RuntimeUtils.To3DAttributes(Vector3.zero));
                
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
        
        // REGRET
        public void PlayRegret()
        {
            _regret.start();
        }

        public void StopRegret()
        {
            _regret.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        } 
        
        // OMINOUS
        public void PlayOminous()
        {
            _ominous.start();
        }

        public void StopOminous()
        {
            _ominous.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
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

        public void PlayPhone()
        {
            // _phone.start();
        }
        
         
        
        // im setting all parametrs here
        //public void SetLowPass(float value)
       // {
         //   _currentAmbient.setParameterByName("LowPass", value);
        //}
        
    }
    
    
}