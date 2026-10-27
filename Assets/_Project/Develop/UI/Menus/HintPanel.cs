using _Project.Develop.UI.HUD;
using PrimeTween;
using UnityEngine;

namespace _Project.Develop.UI.Menus
{
    public class HintPanel : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button hintButton;
        [SerializeField] private UnityEngine.UI.Button hintPanelButton;
        [SerializeField] private CanvasGroup overlayGroup;
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.2f;
        [SerializeField, Min(0f)] private float hintAppearDelay = 0.35f;
        [SerializeField, Min(0f)] private float hintGameplayDuration = 30f;

        private Tween _fadeTween;
        private Sequence _hintSequence;
        private CanvasGroup _hintGroup;
        private Vector3 _hintScale;
        private Quaternion _hintRotation;
        private MainMenu _mainMenu;
        private UIBar _bar;
        private CanvasGroup _barGroup;
        private float _hintAlpha;
        private float _gameplayTime;
        private bool _appeared;
        private bool _expired;
        private bool _pausedForHint;
        private float _previousTimeScale;

        private void Awake()
        {
            if (hintButton == null || hintPanelButton == null || overlayGroup == null)
            {
                Debug.LogError("TutorialMenu: assign Hint, Hint Panel and Overlay references.", this);
                enabled = false;
                return;
            }

            _hintScale = hintButton.transform.localScale;
            _hintRotation = hintButton.transform.localRotation;
            _hintGroup = hintButton.GetComponent<CanvasGroup>();
            if (_hintGroup == null) _hintGroup = hintButton.gameObject.AddComponent<CanvasGroup>();
            _mainMenu = FindFirstObjectByType<MainMenu>(FindObjectsInactive.Include);
            _bar = FindFirstObjectByType<UIBar>(FindObjectsInactive.Include);
            if (_bar != null) _barGroup = _bar.GetComponent<CanvasGroup>();

            ResetPanel();
        }

        private void OnEnable()
        {
            hintButton.onClick.AddListener(Show);
            hintPanelButton.onClick.AddListener(Hide);
            hintButton.gameObject.SetActive(false);
            hintButton.interactable = false;
            _hintGroup.alpha = 0f;
            _hintAlpha = 0f;
            _appeared = false;
        }

        private void Update()
        {
            var hudVisible = _bar != null && _bar.Showing && _barGroup != null && _barGroup.alpha >= 0.99f;
            if (_hintSequence.isAlive) _hintSequence.isPaused = !_pausedForHint && !hudVisible;
            if (_expired || !hudVisible || _mainMenu == null || !_mainMenu.IsGameplayActive || Time.timeScale <= 0f) return;

            _gameplayTime += Time.deltaTime;
            if (_gameplayTime >= hintGameplayDuration)
            {
                HideHint();
                return;
            }

            if (!_appeared) ShowHint();
        }

        private void LateUpdate()
        {
            // Follow UIBar's tween directly so fades use exactly the same timing and easing.
            _hintGroup.alpha = _hintAlpha * (_barGroup != null ? _barGroup.alpha : 0f);
            var visible = !_expired && _bar != null && _bar.Showing && _hintGroup.alpha >= 0.99f;
            _hintGroup.interactable = visible;
            _hintGroup.blocksRaycasts = visible;
        }

        private void ShowHint()
        {
            _appeared = true;
            hintButton.gameObject.SetActive(true);
            hintButton.transform.localScale = _hintScale * 0.85f;
            _hintSequence = Sequence.Create()
                .ChainDelay(hintAppearDelay)
                .Chain(Tween.Custom(_hintAlpha, 1f, 0.25f, value => _hintAlpha = value, Ease.OutCubic))
                .Group(Tween.Scale(hintButton.transform, _hintScale, 0.3f, Ease.OutCubic))
                .ChainCallback(() => hintButton.interactable = true)
                .Chain(Tween.PunchLocalRotation(hintButton.transform, new Vector3(0f, 0f, 15f),
                    duration: 0.7f, frequency: 5f))
                .Group(Tween.PunchScale(hintButton.transform, _hintScale * 0.12f,
                    duration: 0.7f, frequency: 5f));
        }

        private void OnDisable()
        {
            if (hintButton != null) hintButton.onClick.RemoveListener(Show);
            if (hintPanelButton != null) hintPanelButton.onClick.RemoveListener(Hide);
            StopHintAnimation();
            ResetPanel();
            ResumeGameplay();
        }

        private void Show()
        {
            if (_expired || !hintButton.interactable || overlayGroup.interactable) return;
            if (_fadeTween.isAlive) _fadeTween.Stop();
            StopHintAnimation();
            hintButton.interactable = false;
            _hintAlpha = 1f;
            _previousTimeScale = Time.timeScale;
            _pausedForHint = true;
            Time.timeScale = 0f;
            G.Get<EventBus>().Pause?.Invoke();
            _hintSequence = Sequence.Create(useUnscaledTime: true)
                .Chain(Tween.Scale(hintButton.transform, _hintScale * 0.9f, 0.15f, Ease.OutCubic))
                .Group(Tween.LocalRotation(hintButton.transform,
                    _hintRotation * Quaternion.Euler(0f, 0f, -3f), 0.15f, Ease.OutCubic));

            overlayGroup.gameObject.SetActive(true);
            overlayGroup.blocksRaycasts = true;
            overlayGroup.interactable = true;
            _fadeTween = Tween.Alpha(overlayGroup, endValue: 1f, duration: fadeDuration,
                ease: Ease.OutCubic, useUnscaledTime: true);
        }

        private void Hide()
        {
            if (!overlayGroup.interactable) return;
            if (_fadeTween.isAlive) _fadeTween.Stop();

            overlayGroup.interactable = false;
            if (!_expired)
            {
                StopHintAnimation(resetRotation: false);
                hintButton.interactable = true;
                _hintSequence = Sequence.Create(useUnscaledTime: true)
                    .Chain(Tween.Scale(hintButton.transform, _hintScale, 0.15f, Ease.OutCubic))
                    .Group(Tween.LocalRotation(hintButton.transform, _hintRotation, 0.15f, Ease.OutCubic));
            }
            _fadeTween = Tween.Alpha(overlayGroup, endValue: 0f, duration: fadeDuration,
                    ease: Ease.InOutSine, useUnscaledTime: true)
                .OnComplete(() =>
                {
                    overlayGroup.blocksRaycasts = false;
                    ResumeGameplay();
                });
        }

        private void HideHint()
        {
            _expired = true;
            StopHintAnimation();
            hintButton.interactable = false;
            _hintSequence = Sequence.Create()
                .Chain(Tween.Custom(_hintAlpha, 0f, fadeDuration, value => _hintAlpha = value, Ease.InOutSine))
                .Group(Tween.Scale(hintButton.transform, _hintScale * 0.8f, fadeDuration, Ease.InOutSine))
                .OnComplete(() => hintButton.gameObject.SetActive(false));
        }

        private void ResumeGameplay()
        {
            if (!_pausedForHint) return;

            _pausedForHint = false;
            Time.timeScale = _previousTimeScale;
            G.Get<EventBus>().Resume?.Invoke();
        }

        private void StopHintAnimation(bool resetRotation = true)
        {
            if (_hintSequence.isAlive) _hintSequence.Stop();
            if (hintButton == null) return;
            if (resetRotation) hintButton.transform.localRotation = _hintRotation;
        }

        private void ResetPanel()
        {
            if (_fadeTween.isAlive) _fadeTween.Stop();
            if (overlayGroup == null) return;

            overlayGroup.alpha = 0f;
            overlayGroup.interactable = false;
            overlayGroup.blocksRaycasts = false;
        }
    }
}
