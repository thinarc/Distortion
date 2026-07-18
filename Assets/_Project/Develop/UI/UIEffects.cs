using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using _Project.Develop.Input;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace _Project.Develop.UI
{
    [Flags]
    public enum UIEffectType
    {
        None = 0,

        HoverScale = 1 << 0,
        PressScale = 1 << 1,

        HoverTilt = 1 << 2,
        HoverMove = 1 << 3,

        Floating = 1 << 4,
        Breathing = 1 << 5,
        Wobble = 1 << 6,
    }

    public class UIEffects : MonoBehaviour, IPointerEnterHandler,  IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private UIEffectType effects;
        [SerializeField] private bool unscaledTime;

        [BoxGroup("Scale"), ShowIf(nameof(HasHoverScale)), SerializeField, Space(5)] private float hoverScale = 1.1f;
        [BoxGroup("Scale"), ShowIf(nameof(HasPressScale)), SerializeField] private float pressScale = 0.9f;
        [BoxGroup("Scale"), ShowIf(nameof(HasHoverOrPressScale)), SerializeField] private float scaleDuration = 0.15f;

        [BoxGroup("Hover"), ShowIf(nameof(HasHoverTilt)), SerializeField, Space(5)] private float tiltAmount = 8f;
        [BoxGroup("Hover"), ShowIf(nameof(HasHoverMove)), SerializeField] private float moveAmount = 10f;
        [BoxGroup("Hover"), ShowIf(nameof(HasHoverTiltOrMove)), SerializeField] private float smoothSpeed = 12f;
        
        [BoxGroup("Floating"), ShowIf(nameof(HasFloating)), SerializeField, Space(5)] private float floatDistance = 10f;
        [BoxGroup("Floating"), ShowIf(nameof(HasFloating)), SerializeField] private float floatDuration = 1f;
        
        [BoxGroup("Breathing"), ShowIf(nameof(HasBreathing)), SerializeField, Space(5)] private float breathingScale = 1.05f;
        [BoxGroup("Breathing"), ShowIf(nameof(HasBreathing)), SerializeField] private float breathingDuration = 1f;
        
        [BoxGroup("Wobble"), ShowIf(nameof(HasWobble)), SerializeField, Space(5)] private float wobbleAngle = 3f;
        [BoxGroup("Wobble"), ShowIf(nameof(HasWobble)), SerializeField] private float wobbleDuration = 0.08f;

        private RectTransform _rect;
        private Canvas _canvas;
        
        private InputController _inputController;

        private Vector3 _baseScale;
        private Vector3 _basePosition;

        private bool _hovering;

        private Quaternion _targetRotation;
        private Vector3 _targetPosition;

        private bool Has(UIEffectType effect)
        {
            return (effects & effect) != 0;
        }

        private void Start()
        {
            _rect = (RectTransform)transform;
            _canvas = GetComponentInParent<Canvas>();
            
            _inputController = G.Get<InputController>();

            _baseScale = transform.localScale;
            _basePosition = transform.localPosition;

            _targetRotation = Quaternion.identity;
            _targetPosition = _basePosition;
            
            if (Has(UIEffectType.Floating))
            {
                Tween.LocalPositionY(transform, endValue: _basePosition.y + floatDistance, duration: floatDuration,
                    ease: Ease.InOutSine, cycles: -1, CycleMode.Yoyo, useUnscaledTime: unscaledTime);
            }

            if (Has(UIEffectType.Breathing))
            {
                Tween.Scale(transform, endValue: _baseScale * breathingScale, duration: breathingDuration,
                    ease: Ease.InOutSine, cycles: -1, CycleMode.Yoyo, useUnscaledTime: unscaledTime);
            }

            if (Has(UIEffectType.Wobble))
            {
                Tween.LocalRotation(transform, endValue: new Vector3(0, 0, wobbleAngle), duration: wobbleDuration,
                    ease: Ease.Linear, cycles: -1, CycleMode.Yoyo, useUnscaledTime: unscaledTime);
            }
        }

        private void Update()
        {
            var useTilt = Has(UIEffectType.HoverTilt);
            var useMove = Has(UIEffectType.HoverMove);

            if (!useTilt && !useMove) return;

            if (!_hovering)
            {
                if (useTilt)
                {
                    _rect.localRotation = Quaternion.Lerp(_rect.localRotation, Quaternion.identity, Time.unscaledDeltaTime * smoothSpeed);
                }

                if (useMove)
                {
                    _rect.localPosition = Vector3.Lerp(_rect.localPosition, _basePosition, Time.unscaledDeltaTime * smoothSpeed);
                }

                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rect,
                _inputController.Player.Player.MousePosition.ReadValue<Vector2>(),
                _canvas.worldCamera,
                out var point);

            var x = Mathf.Clamp(point.x / (_rect.rect.width * 0.5f), -1f, 1f);
            var y = Mathf.Clamp(point.y / (_rect.rect.height * 0.5f), -1f, 1f);

            if (useTilt)
            {
                _targetRotation = Quaternion.Euler(-y * tiltAmount, x * tiltAmount, 0);
                _rect.localRotation = Quaternion.Lerp(_rect.localRotation, _targetRotation, Time.unscaledDeltaTime * smoothSpeed);
            }

            if (!useMove) return;
            
            _targetPosition = _basePosition + new Vector3(x * moveAmount, y * moveAmount, 0);
            _rect.localPosition = Vector3.Lerp(_rect.localPosition, _targetPosition, Time.unscaledDeltaTime * smoothSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;

            if (!Has(UIEffectType.HoverScale)) return;

            Tween.StopAll(transform);
            
            Tween.Scale(transform, endValue: _baseScale * hoverScale, duration: scaleDuration, useUnscaledTime: unscaledTime);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;

            if (!Has(UIEffectType.HoverScale)) return;
                
            Tween.StopAll(transform);

            Tween.Scale(transform, endValue: _baseScale, duration: scaleDuration, useUnscaledTime: unscaledTime);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Has(UIEffectType.PressScale)) return;
            
            Tween.Scale(transform, endValue: _baseScale * pressScale, duration: 0.05f, useUnscaledTime: unscaledTime);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!Has(UIEffectType.PressScale)) return;

            var target = _hovering && Has(UIEffectType.HoverScale) ? hoverScale : 1f;
            
            Tween.Scale(transform, endValue: _baseScale * target, duration: 0.1f, useUnscaledTime: unscaledTime);
        }

        private void OnDestroy()
        {
            Tween.StopAll(transform);
        }
        
        private bool HasHoverScale => Has(UIEffectType.HoverScale);
        private bool HasPressScale => Has(UIEffectType.PressScale);
        private bool HasHoverOrPressScale => Has(UIEffectType.HoverScale) || Has(UIEffectType.PressScale);
        
        private bool HasHoverTilt => Has(UIEffectType.HoverTilt);
        private bool HasHoverMove => Has(UIEffectType.HoverMove);
        private bool HasHoverTiltOrMove => Has(UIEffectType.HoverTilt) || Has(UIEffectType.HoverMove);
        
        private bool HasFloating => Has(UIEffectType.Floating);
        private bool HasBreathing => Has(UIEffectType.Breathing);
        private bool HasWobble => Has(UIEffectType.Wobble);
    }
}