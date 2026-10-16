using _Project.Develop.Editing.Photo;
using _Project.Develop.Input;
using _Project.Develop.UI;
using _Project.Develop.UI.Cursor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Project.Develop.Editing.Tools
{
    public abstract class PickupTool : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private Vector2 pickupOffset;
        [SerializeField, Space(5)] private UIEffects photoEffects;
        
        private Image _image;
        private Canvas _canvas;

        private UIEffects _effects;
        private CursorToolTrigger _toolTrigger;
        
        protected Canvas Canvas => _canvas;
        
        protected bool Picked { get; private set; }

        protected bool CanChangePhotoEffect { get; set; } = true;
        
        private bool HasEffect { get; set; } = true;
        private bool Disabled { get; set; }

        protected virtual void Start()
        {
            _image = GetComponent<Image>();
            _canvas = GetComponentInParent<Canvas>();
            
            _effects = GetComponent<UIEffects>();
            _toolTrigger = GetComponent<CursorToolTrigger>();

            G.Get<EventBus>().StartEditing += OnStartEditing;
            G.Get<EventBus>().SpotsFinished += OnSpotsFinished;
            G.Get<EventBus>().EndEditing += OnEndEditing;
            
            G.Get<InputController>().Player.Player.MousePosition.performed += OnMousePosition;
        }

        private void OnStartEditing()
        {
            _effects.SetEffect(UIEffectType.PressScale, true);
            _effects.SetEffect(UIEffectType.HoverTilt, true);
            
            _toolTrigger.Enable();
            
            HasEffect = true;
            Disabled = false;
            
            CanChangePhotoEffect = true;
        }

        protected virtual void OnSpotsFinished()
        {
            HasEffect = false;
        }

        private void OnEndEditing()
        {
            _effects.SetEffect(UIEffectType.PressScale, false);
            _effects.SetEffect(UIEffectType.HoverTilt, false);
            
            _toolTrigger.Disable();
            
            HasEffect = false;
            Disabled = true;
        }

        private void OnMousePosition(InputAction.CallbackContext obj)
        {
            if (!Picked) return;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                obj.ReadValue<Vector2>(),
                _canvas.worldCamera,
                out var localPoint
            );
            
            transform.localPosition = localPoint + pickupOffset;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Disabled) return;
            PickUp();
        }

        protected virtual void PickUp()
        {
            _image.raycastTarget = false;
            Picked = true;
            
            transform.SetAsLastSibling();
            _effects.enabled = false;

            photoEffects.enabled = HasEffect;
            
            G.Get<InputController>().Player.Player.Click.canceled += Drop;
        }

        protected virtual void Drop(InputAction.CallbackContext obj)
        {
            _image.raycastTarget = true;
            Picked = false;
            
            _effects.enabled = true;
            if (CanChangePhotoEffect) photoEffects.enabled = !HasEffect;

            G.Get<InputController>().Player.Player.Click.canceled -= Drop;
        }
        
        protected virtual void OnSpotEnter(EditingSpot spot)
        {
            spot.Clean();
        }

        protected virtual void OnDestroy()
        {
            G.Get<EventBus>().StartEditing -= OnStartEditing;
            G.Get<EventBus>().SpotsFinished -= OnSpotsFinished;
            G.Get<EventBus>().EndEditing -= OnEndEditing;
            
            G.Get<InputController>().Player.Player.MousePosition.performed -= OnMousePosition;
        }
    }
}