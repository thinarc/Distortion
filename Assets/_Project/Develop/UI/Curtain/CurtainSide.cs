using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Develop.UI.Curtain
{
    public class CurtainSide : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private bool rightSide;
        
        private RectTransform _rect;
        
        private float _disclosure;
        
        private const float MinScale = 0.4f;
        private const float Sensitivity = 0.0025f;
        
        private bool _dragging;
        private bool _placed;
        private bool _disabled;
        
        public float Disclosure => _disclosure;

        private void Start()
        {
            _rect = GetComponent<RectTransform>();
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging || _disabled) return;
            var delta = eventData.delta.x * Sensitivity;
            _disclosure = !rightSide ? Mathf.Clamp01(_disclosure - delta) : Mathf.Clamp01(_disclosure + delta);
            Apply();
        }

        private void Apply()
        {
            var scale = _rect.localScale;
            scale.x = Mathf.Lerp(1f, MinScale, _disclosure);
            _rect.localScale = scale;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            _dragging = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _dragging = false;
        }

        public void Disable() => _disabled = true;
        
        public void Enable() => _disabled = false;
    }
}