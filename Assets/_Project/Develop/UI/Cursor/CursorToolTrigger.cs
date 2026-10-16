using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Develop.UI.Cursor
{
    public class CursorToolTrigger : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private bool _dragging;
        private bool _placed;

        private bool _disabled;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_disabled) return;
            _placed = true;
            G.Get<UICursor>().SetToolMode();
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (_disabled) return;
            _dragging = true;
            G.Get<UICursor>().SetToolMode();
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            _dragging = false;
            if (!_placed) G.Get<UICursor>().SetDefaultMode();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _placed = false;
            if (!_dragging) G.Get<UICursor>().SetDefaultMode();
        }

        public void Disable() => _disabled = true;
        
        public void Enable() => _disabled = false;
    }
}