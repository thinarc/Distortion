using _Project.Develop.Anomaly;
using UnityEngine;
using Cysharp.Threading.Tasks;
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
        private Sprite _initialFront;
        private Sprite _initialNormal;
        private Vector3 _initialPosition;
        private Vector3 _initialScale;
        private Quaternion _initialRotation;

        public void CaptureInitialState()
        {
            _initialFront = Image.sprite;
            _initialNormal = normalFront;
            var effects = GetComponentInParent<_Project.Develop.UI.UIEffects>(true);
            _initialPosition = effects.transform.localPosition;
            _initialScale = effects.transform.localScale;
            _initialRotation = effects.transform.localRotation;
        }

        public void ResetPhoto()
        {
            Ano?.DeactivateSource().Forget();
            Ano = null;
            normalFront = _initialNormal;
            Image.sprite = _initialFront;
            var color = Image.color;
            color.a = 0f;
            Image.color = color;
            var effects = GetComponentInParent<_Project.Develop.UI.UIEffects>(true);
            effects.transform.localPosition = _initialPosition;
            effects.transform.localScale = _initialScale;
            effects.transform.localRotation = _initialRotation;
            effects.SetEffect(_Project.Develop.UI.UIEffectType.HoverMove, false);
            effects.enabled = false;
            gameObject.SetActive(false);
        }

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
