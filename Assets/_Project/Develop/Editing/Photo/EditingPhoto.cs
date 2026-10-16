using _Project.Develop.Anomaly;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace _Project.Develop.Editing.Photo
{
    public class EditingPhoto : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private LocalizedSprite backdropPhoto;
        [SerializeField] private int number;

        [SerializeField, Space(5)] private Sprite normalFront;

        public Sprite FrontPhoto { get; private set; }
        
        public AnomalyObject Ano { get; set; }
        
        public Sprite BackdropPhoto { get; private set; }
        
        public Sprite NormalFront
        {
            get => normalFront;
            set => normalFront = value;
        }
        
        public int Number => number;

        private Image _image;

        public Image Image
        {
            get
            {
                if (_image == null) _image = GetComponent<Image>();
                return _image;
            }
        }

        private void Start()
        {
            FrontPhoto = Image.sprite;
            backdropPhoto.AssetChanged += OnBackdropChanged;
        }
        
        private void OnBackdropChanged(Sprite newSprite) => BackdropPhoto = newSprite;

        public void OnPointerEnter(PointerEventData eventData) => G.Get<EventBus>().PhotoEnter?.Invoke();
        
        public void OnPointerExit(PointerEventData eventData) => G.Get<EventBus>().PhotoExit?.Invoke();
        
        public void OnPointerDown(PointerEventData eventData) => G.Get<EventBus>().PhotoDown?.Invoke();

        private void OnDestroy() => backdropPhoto.AssetChanged -= OnBackdropChanged;
    }
}