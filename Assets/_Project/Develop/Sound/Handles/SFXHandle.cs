using System;
using FMOD.Studio;
using FMODUnity;

namespace _Project.Develop.Sound.Handles
{
    public class SFXHandle : IDisposable
    {
        // ===== FIELDS =====

        private EventInstance _lightout;


        // ===== INIT / DISPOSE =====

        public void Init()
        {
            _lightout = RuntimeManager.CreateInstance("event:/SFX/Event/LightOut");
        }

        public void Dispose()
        {
            if (_lightout.isValid())
            {
                _lightout.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _lightout.release();
            }
        }


        // ===== LIGHT OUT (звук перегорания лампы) =====

        public void PlayLightOut()
        {
            _lightout.start();
        }

        public void StopLightOut()
        {
            _lightout.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }
}