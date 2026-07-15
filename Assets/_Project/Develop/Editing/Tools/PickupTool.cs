using _Project.Develop.Editing.Photo;
using _Project.Develop.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Project.Develop.Editing.Tools
{
    public abstract class PickupTool : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private Vector2 pickupOffset;
        
        private Image _image;
        private Canvas _canvas;
        
        protected Canvas Canvas => _canvas;
        
        protected bool Picked { get; private set; }

        protected virtual void Start()
        {
            _image = GetComponent<Image>();
            _canvas = GetComponentInParent<Canvas>();

            G.Get<InputController>().Player.Player.MousePosition.performed += OnMousePosition;
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

        public void OnPointerDown(PointerEventData eventData) => PickUp();

        protected virtual void PickUp()
        {
            _image.raycastTarget = false;
            Picked = true;
            
            transform.SetAsLastSibling();
            
            G.Get<InputController>().Player.Player.Click.canceled += Drop;
        }

        protected virtual void Drop(InputAction.CallbackContext obj)
        {
            _image.raycastTarget = true;
            Picked = false;
            
            G.Get<InputController>().Player.Player.Click.canceled -= Drop;
        }
        
        protected virtual void OnSpotEnter(EditingSpot spot)
        {
            spot.Clean();
        }

        private void OnDestroy()
        {
            G.Get<InputController>().Player.Player.MousePosition.performed -= OnMousePosition;
        }
    }
}