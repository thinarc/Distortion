using _Project.Develop.Sound;
using _Project.Develop.UI.Cursor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Develop.UI.Curtain
{
    public class CurtainSide : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private bool rightSide;
        
        [SerializeField, Space(5)] private RectTransform graphicRect;
        
        private RectTransform _rect;
        private CursorToolTrigger _toolTrigger;
        
        private float _disclosure;
        
        private const float MinScale = 0.4f;
        private const float Sensitivity = 0.0025f;
        
        private bool _dragging;
        private bool _disabled;
        
        public float Disclosure => _disclosure;

        private void Start()
        {
            _rect = GetComponent<RectTransform>();
            _toolTrigger = GetComponent<CursorToolTrigger>();
            
            G.Get<EventBus>().Pause += OnPause;
            G.Get<EventBus>().Resume += OnResume;
            G.Get<EventBus>().GoodEnd += OnSceneReady;
            G.Get<EventBus>().SceneReady += OnSceneReady;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            _dragging = true;
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging || _disabled) return;
            
            G.Get<EventBus>().SideDragging?.Invoke();
            
            var delta = eventData.delta.x * Sensitivity;
            _disclosure = !rightSide ? Mathf.Clamp01(_disclosure - delta) : Mathf.Clamp01(_disclosure + delta);
            Apply();
            
            G.Get<SoundController>().ActionHandle.AddCurtainVelocity(Mathf.Abs(delta));
            
            if (_disclosure <= 0f || _disclosure >= 1f)
                G.Get<SoundController>().ActionHandle.StopCurtainImmediate();
            else
                G.Get<SoundController>().ActionHandle.PlayCurtain();
        }

        private void Apply()
        {
            var scale = _rect.localScale;
            scale.x = Mathf.Lerp(1f, MinScale, _disclosure);
            _rect.localScale = scale;
            
            graphicRect.localScale = scale;
        }
        
        private void OnSceneReady()
        {
            _disclosure = 0f;
            Apply();
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            _dragging = false;
            G.Get<SoundController>().ActionHandle.StopCurtainImmediate();
        }

        private void OnPause()
        {
            _toolTrigger.Disable();
        }

        private void OnResume()
        {
            if (!_disabled)
                _toolTrigger.Enable();
        }

        public void Disable()
        {
            _disabled = true;
            _toolTrigger.Disable();
        }

        public void Enable()
        {
            _disabled = false;
            _toolTrigger.Enable();
        }

        private void OnDestroy()
        {
            G.Get<EventBus>().Pause -= OnPause;
            G.Get<EventBus>().Resume -= OnResume;
            G.Get<EventBus>().GoodEnd -= OnSceneReady;
            G.Get<EventBus>().SceneReady -= OnSceneReady;
        }
    }
}