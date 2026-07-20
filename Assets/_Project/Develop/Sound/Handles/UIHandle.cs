// connect library
using FMOD.Studio;
using FMODUnity;

// address 
namespace _Project.Develop.Sound.Handles
{
    // class named
    public class UIHandle
        
        
        // set variables
    {
        private EventInstance _scroll;
        private EventInstance _click;
        private EventInstance _exit;
        private EventInstance _hover;

        public void Initialize()
            // create a live copy for event to control it
        {
            _scroll = RuntimeManager.CreateInstance("event:/UI/Scroll");
            _click = RuntimeManager.CreateInstance("event:/UI/Click");
            _exit = RuntimeManager.CreateInstance("event:/UI/Exit");
            _hover = RuntimeManager.CreateInstance("event:/UI/Hover");
        }

        
        // external buttons
        //Just like play
        public void PlayScroll()
        {
            _scroll.start();
        }

        public void PlayClick()
        {
            _click.start();
        }

        public void PlayHover()
        {
            _hover.start();
        }

        public void PlayExit()
        {
            _exit.start();
        }
        
        
    }
    
    
}