using System;
using FMOD.Studio;
using FMODUnity;
using _Project.Develop.UI.Menus;

namespace _Project.Develop.Sound.Handles
{
    public class UIHandle : IDisposable
    {
        // ===== FIELDS =====

        private EventInstance _scroll;
        private EventInstance _click;
        private EventInstance _exit;
        private EventInstance _hover;

        private EventInstance _phoneRingInstance;


        // ===== INIT / DISPOSE =====

        public void Init()
        {
            _scroll = RuntimeManager.CreateInstance("event:/UI/Scroll");
            _click = RuntimeManager.CreateInstance("event:/UI/Click");
            _exit = RuntimeManager.CreateInstance("event:/UI/Exit");
            _hover = RuntimeManager.CreateInstance("event:/UI/Hover");
        }

        public void Dispose()
        {
            Release(_scroll);
            Release(_click);
            Release(_exit);
            Release(_hover);

            StopPhoneRings();
        }

        private static void Release(EventInstance instance)
        {
            if (!instance.isValid()) return;
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
        }


        // ===== BUTTON SOUNDS =====

        public void PlayScroll() { _scroll.start(); }
        public void PlayClick() { _click.start(); }
        public void PlayHover() { _hover.start(); }
        public void PlayExit() { _exit.start(); }

        public void PhoneRings()
        {
            StopPhoneRings();
            
            _phoneRingInstance = RuntimeManager.CreateInstance("event:/UI/Phone");
            _phoneRingInstance.start();

        }

        public void StopPhoneRings()
        {
            if (_phoneRingInstance.isValid())
            {
                _phoneRingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                _phoneRingInstance.release();
                _phoneRingInstance = default;
            }
        }


        // ===== REST DIALOG =====

        public void RestEnter()
        {
            var instance = RuntimeManager.CreateInstance("event:/UI/RestEnter");
            instance.start();
            instance.release();
        }
        
        public void TakesPills()
        {
            var instance = RuntimeManager.CreateInstance("event:/UI/TakesPills");
            instance.start();
            instance.release(); 
        }
        
        
    }
}