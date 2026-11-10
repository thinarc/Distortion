using System.Collections;
using PrimeTween;
using TMPEffects.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.UI
{
    [RequireComponent(typeof(CanvasGroup), typeof(Image), typeof(Button))]
    public partial class DialogueController : MonoBehaviour, IService
    {
        [SerializeField, Min(0.01f)] private float appearDuration = 0.85f;
        [SerializeField, Min(0.01f)] private float disappearDuration = 0.65f;
        [SerializeField, Min(0f)] private float slideDistance = 4f;
        [SerializeField, Range(0.9f, 1f)] private float hiddenScale = 1f;

        private CanvasGroup _group;
        private RectTransform _rect;
        private Vector2 _shownPosition;
        private Vector3 _shownScale;
        private TMP_Text _text;
        private TMPWriter _writer;
        private Button _button;
        private EventBus _bus;
        private Tween _fade;
        private float _visibility;
        private Coroutine _prepareLine;
        private string[] _lines;
        private int _index;
        private bool _locationVisible = true;
        private bool _paused;
        private bool _effect;
        private bool _resumeWriting;

        public bool IsActive => _lines != null;
        private bool CanShow => IsActive && _locationVisible && !_paused && !_effect;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            _rect = (RectTransform)transform;
            _shownPosition = _rect.anchoredPosition;
            _shownScale = _rect.localScale;
            _button = GetComponent<Button>();
            _button.targetGraphic = GetComponent<Image>();
            _button.transition = Selectable.Transition.None;
            _button.navigation = new Navigation { mode = Navigation.Mode.None };
            _button.onClick.AddListener(Advance);
            _text = GetComponentInChildren<TMP_Text>(true);
            _writer = _text.GetComponent<TMPWriter>();
            // Match the phone dialogue in the Average ending.
            _writer.DefaultDelays.SetDelay(0.06f);
            _writer.DefaultDelays.SetPunctuationDelay(2.4f, TMPWriter.DelayType.Percentage);
            _writer.WriteOnStart = false;
            _writer.WriteOnNewText = false;
            _writer.StopWriter();
            // Child decorations (including the old next button) must not intercept clicks.
            foreach (var graphic in GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = graphic == _button.targetGraphic;
            ApplyVisibility(0f);
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }

        public void Init()
        {
            if (_bus != null) return;
            _bus = G.Get<EventBus>();
            _bus.Pause += OnPause;
            _bus.Resume += OnResume;
            _bus.EffectApply += OnEffectApply;
            _bus.EffectRemove += OnEffectRemove;
            _bus.PreSceneReset += StopTutorialDialogue;
            _bus.RunReset += ResetTutorialDialogue;
            _bus.SceneReady += RestartTutorialDialogue;
            _bus.SpotsFinished += OnPhotoCleaned;
            _bus.EndEditing += OnPhotoFlipped;
            _bus.ChecklistDown += OnPhotoCompleted;
        }

        public void ShowStartupDialogue() => StartTutorialDialogue();

        public void Show(params string[] lines)
        {
            Close();
            if (lines == null || lines.Length == 0) return;
            _lines = (string[])lines.Clone();
            _index = 0;
            ShowLine();
            UpdateVisibility(appearDuration, Ease.InOutSine);
        }

        private void ShowLine()
        {
            _writer.StopWriter();
            _prepareLine = StartCoroutine(PrepareLine());
        }

        private IEnumerator PrepareLine()
        {
            _text.maxVisibleCharacters = 0;
            _text.text = _lines[_index];
            // TMPWriter needs to process the new text before restarting.
            yield return null;
            _text.maxVisibleCharacters = int.MaxValue;
            _writer.RestartWriter();
            _resumeWriting = !CanShow;
            if (_resumeWriting) _writer.StopWriter();
            _prepareLine = null;
        }

        public void Advance()
        {
            if (!CanShow || !_group.interactable || _prepareLine != null) return;
            if (_writer.IsWriting)
            {
                _writer.SkipWriter();
                return;
            }

            if (_index == _lines.Length - 1) MarkTutorialCueRead();
            if (++_index < _lines.Length) ShowLine();
            else
            {
                var completedTutorial = _activeCueCompletesTutorial;
                Close();
                if (completedTutorial) TutorialDialogueCompleted = true;
            }
        }

        public void Close()
        {
            _activeCueCompletesTutorial = false;
            _activeTutorialLastLine = 0;
            if (_prepareLine != null) StopCoroutine(_prepareLine);
            _prepareLine = null;
            _lines = null;
            _resumeWriting = false;
            _writer.StopWriter();
            UpdateVisibility(disappearDuration, Ease.InOutSine);
        }

        public void SetLocationVisible(bool visible, float duration, Ease ease, string location = null)
        {
            if (location != null) ChangeTutorialLocation(location);
            _locationVisible = visible;
            UpdateVisibility(duration, ease);
        }

        private void OnPause() { _paused = true; UpdateVisibility(disappearDuration, Ease.InOutSine); }
        private void OnResume() { _paused = false; UpdateVisibility(appearDuration, Ease.InOutSine); }
        private void OnEffectApply() { _effect = true; UpdateVisibility(disappearDuration, Ease.InOutSine); }
        private void OnEffectRemove() { _effect = false; UpdateVisibility(appearDuration, Ease.InOutSine); }

        private void ApplyVisibility(float alpha)
        {
            alpha = Mathf.Clamp01(alpha);
            _visibility = alpha;
            _group.alpha = Mathf.Pow(alpha, 1.5f);
            _rect.anchoredPosition = _shownPosition + Vector2.down * (slideDistance * (1f - alpha));
            _rect.localScale = _shownScale * Mathf.Lerp(hiddenScale, 1f, alpha);
        }

        private void UpdateVisibility(float duration, Ease ease)
        {
            _fade.Stop();
            _group.interactable = false;
            _group.blocksRaycasts = false;
            var visible = CanShow;
            if (!visible)
            {
                _resumeWriting |= _writer.IsWriting;
                _writer.StopWriter();
            }
            else if (_resumeWriting)
            {
                _resumeWriting = false;
                _writer.StartWriter();
            }

            _fade = Tween.Custom(_visibility, visible ? 1f : 0f, duration, ApplyVisibility,
                    ease, useUnscaledTime: true)
                .OnComplete(() =>
                {
                    _group.interactable = CanShow;
                    _group.blocksRaycasts = CanShow;
                });
        }

        private void OnDestroy()
        {
            if (_button != null) _button.onClick.RemoveListener(Advance);
            _fade.Stop();
            if (_bus == null) return;
            _bus.Pause -= OnPause;
            _bus.Resume -= OnResume;
            _bus.EffectApply -= OnEffectApply;
            _bus.EffectRemove -= OnEffectRemove;
            _bus.PreSceneReset -= StopTutorialDialogue;
            _bus.RunReset -= ResetTutorialDialogue;
            _bus.SceneReady -= RestartTutorialDialogue;
            _bus.SpotsFinished -= OnPhotoCleaned;
            _bus.EndEditing -= OnPhotoFlipped;
            _bus.ChecklistDown -= OnPhotoCompleted;
        }
    }
}
