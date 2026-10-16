using FMOD.Studio;
using FMODUnity;
using System;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace _Project.Develop.Sound.Handles
{
    public class EndingHandle : IDisposable
    {
        private EventInstance _currentStinger;
        public void Init()
        {
            // empty
        }

        public void PlayEndingStinger(EndingType ending)
        {
            StopStinger();
            
            _currentStinger = RuntimeManager.CreateInstance("event:/END/Whoiam");
            Debug.Log($"[EndingHandle] Playing stinger with label: {ending}");
            
            _currentStinger.setParameterByNameWithLabel("Whoiam", ending.ToString());
            _currentStinger.start();
        }

        public void StopStinger()
        {
            if (_currentStinger.isValid())
            {
                _currentStinger.stop(STOP_MODE.ALLOWFADEOUT);
                _currentStinger.release();
                _currentStinger = default;
            }
        }
        public void Dispose()
        {
            StopStinger();
            
        }
        
    }

    public enum EndingType
    {
        ImAnomaly,
        ImFired,
        ImDone
    }
}