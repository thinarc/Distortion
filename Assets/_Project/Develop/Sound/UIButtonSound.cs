using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Develop.Sound
{
    public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
    {
        [SerializeField] private SoundType soundType;
        
        [SerializeField] private bool playHoverSound; 
        
        private enum SoundType { Click, Exit, Scroll }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (playHoverSound)
            {
                G.Get<SoundController>().UIHandle.PlayHover();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            switch (soundType)
            {
                case SoundType.Click:
                    G.Get<SoundController>().UIHandle.PlayClick();
                    break;
                case SoundType.Exit:
                    G.Get<SoundController>().UIHandle.PlayExit();
                    break;
                case SoundType.Scroll:
                    G.Get<SoundController>().UIHandle.PlayScroll();
                    break;
            }
        }
    }
}