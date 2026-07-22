using _Project.Develop.Sound;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Develop.UI
{
    public class UIHoverSound : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            G.Get<SoundController>().UIHandle.PlayHover();
        }
    }
}