using System.Collections.Generic;
using System.Linq;
using _Project.Develop.Input;
using _Project.Develop.UI.Menus;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Project.Develop.UI.HUD
{
    public class UIBar : MonoBehaviour, IService
    {
        [SerializeField] private CanvasGroup pillsGroup;
        
        private Slider _slider;
        private TextMeshProUGUI _percentText;
        
        private CanvasGroup _group;

        private float _currentTime;
        private Ease _currentEase;
        
        private Vector2 _pillsGroupDefaultPos;

        private List<Image> _pillsImages;
        private int _availablePills;
        
        private Tween _pillsGroupTween;
        private bool _readyPill;

        private bool _disabled;
        
        private bool _showPills;

        public bool Showing { get; private set; }

        public float Value => _slider.value;
        
        private void Start()
        {
            _slider = GetComponent<Slider>();
            _percentText = GetComponentInChildren<TextMeshProUGUI>();
            
            _group = GetComponent<CanvasGroup>();
            
            _pillsGroupDefaultPos = pillsGroup.GetComponent<RectTransform>().anchoredPosition;

            _pillsImages = pillsGroup.GetComponentsInChildren<Image>().ToList();
            _availablePills = _pillsImages.Count;

            SetValue(0f);
            
            var eventBus = G.Get<EventBus>();
            
            eventBus.Pause += OnPause;
            eventBus.Resume += OnResume;
            eventBus.EffectApply += OnPause;
            eventBus.EffectRemove += OnResume;

            G.Get<InputController>().Player.Player.Space.performed += OnSpacePerformed;
            G.Get<InputController>().Player.Player.Space.canceled += OnSpaceCanceled;
        }
        
        public void AddValue(float value)
        {
            SetValue(_slider.value + value / 1000);
        }

        private void SetValue(float value)
        {
            value = Mathf.Clamp01(value);
            
            _slider.value = value;
            _percentText.text = (value * 100).ToString("F0") + "%";
        }

        private bool CanApplyPill() => !_disabled && _availablePills > 0;

        private void ApplyPill()
        {
            var pill = _pillsImages.FindLast(p => Mathf.Approximately(p.color.a, 1f));
            Tween.Alpha(pill, endValue: 0f, duration: 0.2f);
            _availablePills--;

            G.Get<RestDialog>().ApplyRest(3f, RestDialog.RestType.Chemical).Forget();
        }
        
        private void OnSpacePerformed(InputAction.CallbackContext obj)
        {
            TryApplyPillAsync().Forget();
        }

        private async UniTaskVoid TryApplyPillAsync()
        {
            if (!CanApplyPill())
            {
                _ = Tween.Scale(
                    pillsGroup.transform,
                    endValue: 1.015f,
                    duration: 0.08f,
                    ease: Ease.OutQuad
                ).Chain(
                    Tween.Scale(
                        pillsGroup.transform,
                        endValue: 1f,
                        duration: 0.08f,
                        ease: Ease.InQuad));

                return;
            }

            await UniTask.Delay(200);
            if (G.Get<InputController>().Player.Player.Space.ReadValue<float>() == 0f) return;
            
            _ = Tween.Alpha(pillsGroup, endValue: 1f, duration: _currentTime, startDelay: 0.1f, ease: _currentEase);
            
            _pillsGroupTween.Stop();
            
            _pillsGroupTween = Tween.Scale(
                pillsGroup.transform,
                endValue: 1.1f,
                duration: Random.Range(2f, 3f)
            ).OnComplete(() =>
            {
                _readyPill = true;

                var randNoise = Random.Range(1.2f, 4.5f);

                _pillsGroupTween = Tween.Custom(
                    0f,
                    1f,
                    1f,
                    _ =>
                    {
                        pillsGroup.GetComponent<RectTransform>().anchoredPosition =
                            _pillsGroupDefaultPos +
                            Random.insideUnitCircle * randNoise;
                    },
                    cycles: -1
                );
            });

            _ = Tween.Custom(
                0f,
                1f,
                _pillsGroupTween.duration,
                _ =>
                {
                    pillsGroup.GetComponent<RectTransform>().anchoredPosition =
                        _pillsGroupDefaultPos +
                        Random.insideUnitCircle * 0.8f;
                }
            ).OnComplete(() =>
            {
                pillsGroup.GetComponent<RectTransform>().anchoredPosition = _pillsGroupDefaultPos;
            });
        }
        
        private void OnSpaceCanceled(InputAction.CallbackContext obj)
        {
            DiscardPill();
        }

        private void DiscardPill()
        {
            _pillsGroupTween.Stop();
            
            if (_readyPill && CanApplyPill())
                ApplyPill();
            
            _readyPill = false;
            
            _pillsGroupTween = Tween.Scale(
                pillsGroup.transform,
                endValue: 1f,
                duration: 0.1f
            ).OnComplete(() =>
            {
                if (pillsGroup.alpha != 0f)
                {
                    Tween.Alpha(
                        pillsGroup,
                        endValue: 0f,
                        duration: _currentTime,
                        startDelay: 0.1f,
                        ease: _currentEase
                    );
                }
            });
        }
        
        public void Show(float time, Ease ease, bool showPill = false)
        {
            Showing = true;
            _showPills = showPill;
            
            if (_disabled) return;
            Tween.Alpha(_group, endValue: 1f, duration: time, ease: ease);
            if (showPill) Tween.Alpha(pillsGroup, endValue: 1f, duration: _currentTime, startDelay: 0.1f, ease: _currentEase);

            _currentTime = time / 2f;
            _currentEase = ease;
        }

        public void Hide(float time, Ease ease)
        {
            Tween.Alpha(_group, endValue: 0f, duration: time, ease: ease);
            Showing = false;

            _showPills = false;
        }
        
        private void OnPause()
        {
            if (_group.alpha != 0f) Tween.Alpha(_group, endValue: 0f, duration: 0.2f, ease: Ease.InOutSine, useUnscaledTime: true);
            
            _pillsGroupTween.Stop();
            
            _readyPill = false;
            _disabled = true;
        }

        private void OnResume()
        {
            if (Showing && !Mathf.Approximately(_group.alpha, 1f))
                Tween.Alpha(_group, endValue: 1f, duration: 0.2f, startDelay: 0.1f, ease: Ease.InOutSine);
            
            if (_showPills && !Mathf.Approximately(pillsGroup.alpha, 1f))
                Tween.Alpha(pillsGroup, endValue: 1f, duration: _currentTime, startDelay: 0.1f, ease: _currentEase);
            
            _disabled = false;
        }

        private void OnDestroy()
        {
            var eventBus = G.Get<EventBus>();
            
            eventBus.Pause -= OnPause;
            eventBus.Resume -= OnResume;
            eventBus.EffectApply -= OnPause;
            eventBus.EffectRemove -= OnResume;
            
            G.Get<InputController>().Player.Player.Space.performed -= OnSpacePerformed;
            G.Get<InputController>().Player.Player.Space.canceled -= OnSpaceCanceled;
        }
    }
}