using _Project.Develop.UI;
using _Project.Develop.UI.Cursor;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Develop.Editing.Photo
{
    public class EditingChecklist : MonoBehaviour, IPointerDownHandler
    {
        private UIEffects _effects;
        private CursorToolTrigger _toolTrigger;

        private bool _disabled;

        private void Start()
        {
            _effects = GetComponent<UIEffects>();
            _toolTrigger = GetComponent<CursorToolTrigger>();
            
            Disable().Forget();
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (_disabled) return;
            G.Get<EventBus>().ChecklistDown?.Invoke();
        }

        public async UniTaskVoid SimulateHover()
        {
            _effects.OnPointerEnter(null);
            await UniTask.Delay(180);
            _effects.OnPointerExit(null);
        }

        public async UniTaskVoid Disable()
        {
            _disabled = true;
            await UniTask.Delay(700);
            
            _effects.SetEffect(UIEffectType.PressScale, false);
            _toolTrigger.Disable();
        }

        public void Enable()
        {
            _effects.SetEffect(UIEffectType.PressScale, true);
            _toolTrigger.Enable();
            
            _disabled = false;
        }
    }
}