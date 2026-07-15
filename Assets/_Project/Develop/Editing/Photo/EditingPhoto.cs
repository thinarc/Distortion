using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Project.Develop.Editing.Photo
{
    public class EditingPhoto : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private Sprite backdropPhoto;
        [SerializeField] private int number;
        
        public Sprite BackdropPhoto => backdropPhoto;

        public int Number => number;
        public bool IsOdd => number == 0;

        private Image _image;

        public Image Image
        {
            get
            {
                if (_image == null) _image = GetComponent<Image>();
                return _image;
            }
        }

        public void OnPointerEnter(PointerEventData eventData) => G.Get<EventBus>().PhotoEnter?.Invoke();
        
        public void OnPointerExit(PointerEventData eventData) => G.Get<EventBus>().PhotoExit?.Invoke();
        
        public void OnPointerDown(PointerEventData eventData) => G.Get<EventBus>().PhotoDown?.Invoke();
    }
}