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
        [SerializeField, Min(0.01f)] private float panelTransitionDuration = 0.22f;
        [SerializeField, Min(0f)] private float hintAppearDelay = 0.35f;
        [SerializeField, Min(0f)] private float hintGameplayDuration = 30f;

        private Sequence _fadeTween;
        private Vector3 _panelScale;
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
        private RectTransform _hintRect;
        private Vector2 _hintDefaultPosition;
        private readonly Vector3[] _hintCorners = new Vector3[4];
        private readonly Vector3[] _pillCorners = new Vector3[4];

        private bool DialogueCompleted => G.Has<DialogueController>() && G.Get<DialogueController>().TutorialDialogueCompleted;

        private void Awake()
        {
            if (hintButton == null || hintPanelButton == null || overlayGroup == null)
            {
                Debug.LogError("TutorialMenu: assign Hint, Hint Panel and Overlay references.", this);
                enabled = false;
                return;
            }

            _hintScale = hintButton.transform.localScale;
            _hintRect = (RectTransform)hintButton.transform;
            _hintDefaultPosition = _hintRect.anchoredPosition;
            _panelScale = hintPanelButton.transform.localScale;
            _hintRotation = hintButton.transform.localRotation;
            _hintGroup = hintButton.GetComponent<CanvasGroup>();
            if (_hintGroup == null) _hintGroup = hintButton.gameObject.AddComponent<CanvasGroup>();
            _mainMenu = FindFirstObjectByType<MainMenu>(FindObjectsInactive.Include);
            _bar = FindFirstObjectByType<UIBar>(FindObjectsInactive.Include);
            if (_bar != null) _barGroup = _bar.GetComponent<CanvasGroup>();

            ResetPanel();
        }

        private void Start()
        {
            G.Get<EventBus>().RunReset += ResetRun;
            G.Get<EventBus>().PreSceneReset += StopForReset;
        }

        private void StopForReset()
        {
            if (_pausedForHint) Time.timeScale = _previousTimeScale;
            _pausedForHint = false;
            StopHintAnimation();
            ResetPanel();
        }

        private void ResetRun()
        {
            StopHintAnimation();
            ResetPanel();
            _gameplayTime = 0f;
            _expired = false;
            _appeared = false;
            _hintAlpha = 0f;
            _hintGroup.alpha = 0f;
            if (_pausedForHint) Time.timeScale = _previousTimeScale;
            _pausedForHint = false;
            hintButton.gameObject.SetActive(false);
        }

        public void ResetTutorial()
        {
            if (_hintGroup == null) return;
            ResumeGameplay();
            ResetRun();
        }

        private void OnDestroy()
        {
            G.Get<EventBus>().RunReset -= ResetRun;
            G.Get<EventBus>().PreSceneReset -= StopForReset;
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
            if (!DialogueCompleted) return;
            var hudVisible = _bar != null && _bar.Showing && !_bar.EmergencyShowing && _barGroup != null && _barGroup.alpha >= 0.99f;
            if (_hintSequence.isAlive) _hintSequence.isPaused = !_expired && !_pausedForHint && !hudVisible;
            if (!TutorialSettings.ShouldShow || _expired || _mainMenu == null || !_mainMenu.IsGameplayActive || Time.timeScale <= 0f) return;

            _gameplayTime += Time.deltaTime;
            if (_gameplayTime >= hintGameplayDuration)
            {
                HideHint();
                return;
            }

            if (!_appeared && hudVisible) ShowHint();
        }

        private void LateUpdate()
        {
            UpdateHintPosition();
            // Follow UIBar's tween directly so fades use exactly the same timing and easing.
            _hintGroup.alpha = _hintAlpha * (DialogueCompleted && TutorialSettings.ShouldShow && _bar != null && !_bar.EmergencyShowing && _barGroup != null ? _barGroup.alpha : 0f);
            var visible = !_expired && _bar != null && _bar.Showing && _hintGroup.alpha >= 0.99f;
            _hintGroup.interactable = visible;
            _hintGroup.blocksRaycasts = visible;
        }

        private void UpdateHintPosition()
        {
            if (_hintRect == null) return;
            var position = _hintRect.anchoredPosition;
            position.y = Mathf.MoveTowards(position.y, GetHintTargetY(), Time.unscaledDeltaTime * 700f);
            _hintRect.anchoredPosition = position;
        }

        private float GetHintTargetY()
        {
            var targetY = _hintDefaultPosition.y;
            if (_bar != null && _bar.PillsVisible)
            {
                var parent = _hintRect.parent;
                _hintRect.GetWorldCorners(_hintCorners);
                _bar.PillsRect.GetWorldCorners(_pillCorners);
                var hintBounds = new Bounds(parent.InverseTransformPoint(_hintCorners[0]), Vector3.zero);
                var pillBounds = new Bounds(parent.InverseTransformPoint(_pillCorners[0]), Vector3.zero);
                for (var i = 1; i < 4; i++)
                {
                    hintBounds.Encapsulate(parent.InverseTransformPoint(_hintCorners[i]));
                    pillBounds.Encapsulate(parent.InverseTransformPoint(_pillCorners[i]));
                }
                var defaultOffset = _hintDefaultPosition.y - _hintRect.anchoredPosition.y;
                if (hintBounds.max.x > pillBounds.min.x && hintBounds.min.x < pillBounds.max.x &&
                    hintBounds.max.y + defaultOffset > pillBounds.min.y &&
                    hintBounds.min.y + defaultOffset < pillBounds.max.y)
                    targetY -= hintBounds.max.y + defaultOffset - pillBounds.min.y + 16f;
            }
            return targetY;
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
            if (!DialogueCompleted || !TutorialSettings.ShouldShow || _expired || !hintButton.interactable || overlayGroup.interactable) return;
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

            if (overlayGroup.alpha <= 0f)
                hintPanelButton.transform.localScale = _panelScale * 0.92f;
            overlayGroup.gameObject.SetActive(true);
            overlayGroup.blocksRaycasts = true;
            overlayGroup.interactable = true;
            _fadeTween = Sequence.Create(useUnscaledTime: true)
                .Chain(Tween.Alpha(overlayGroup, endValue: 1f, duration: panelTransitionDuration,
                    ease: Ease.InQuad))
                .Group(Tween.Scale(hintPanelButton.transform, _panelScale, panelTransitionDuration,
                    Ease.OutCubic));
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
            _fadeTween = Sequence.Create(useUnscaledTime: true)
                .Chain(Tween.Alpha(overlayGroup, endValue: 0f, duration: panelTransitionDuration,
                    ease: Ease.OutQuad))
                .Group(Tween.Scale(hintPanelButton.transform, _panelScale * 0.92f,
                    panelTransitionDuration, Ease.InOutSine))
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
            if (hintPanelButton != null) hintPanelButton.transform.localScale = _panelScale;
            overlayGroup.interactable = false;
            overlayGroup.blocksRaycasts = false;
        }
    }
}
