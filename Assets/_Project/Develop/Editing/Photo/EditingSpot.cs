using _Project.Develop.Editing.Tools;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Project.Develop.Editing.Photo
{
    public class EditingSpot : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private SpotType spotType;

        private bool _cleaned;

        private enum SpotType { Dust, Dirt, Both }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_cleaned) return;
            
            switch (spotType)
            {
                case SpotType.Dust:
                    G.Get<EventBus>().DustEnter?.Invoke(this);
                    break;
                case SpotType.Dirt:
                    G.Get<EventBus>().DirtEnter?.Invoke(this);
                    break;
                case SpotType.Both:
                    G.Get<EventBus>().DustEnter?.Invoke(this);
                    G.Get<EventBus>().DirtEnter?.Invoke(this);
                    break;
            }
        }

        public void Clean()
        {
            _cleaned = true;
            Tween.Alpha(GetComponent<Image>(), endValue: 0f, duration: 0.15f).OnComplete(() => Destroy(gameObject));
        }
    }
}