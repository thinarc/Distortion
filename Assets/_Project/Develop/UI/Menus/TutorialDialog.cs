using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPEffects.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

namespace _Project.Develop.UI.Menus
{
    public class TutorialDialog : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.25f;
        [SerializeField, Min(0.01f)] private float appearDuration = 0.65f;
        [SerializeField, Min(1)] private int trailingCharacters = 10;
        [SerializeField, Min(0.01f)] private float characterDelay = 0.055f;

        private TMP_Text _text;
        private TMPWriter _writer;
        private LocalizeStringEvent _localizedText;
        private CanvasGroup _group;
        private Tween _fade;
        private bool _shown;
        private bool _requested;
        private int _revision;

        private bool DialogueCompleted => G.Has<DialogueController>() && G.Get<DialogueController>().TutorialDialogueCompleted;

        private void Init()
        {
            if (_writer != null) return;
            _text = GetComponent<TMP_Text>();
            _writer = GetComponent<TMPWriter>();
            _localizedText = GetComponent<LocalizeStringEvent>();
            _group = GetComponent<CanvasGroup>();
            if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
            _group.interactable = false;
            _group.blocksRaycasts = false;
            _text.raycastTarget = false;
            // Localization may update the text before this tutorial step is reached.
            _writer.WriteOnStart = false;
            _writer.WriteOnNewText = false;
            _writer.enabled = false;
        }

        public void ShowOnce()
        {
            if (!DialogueCompleted || !TutorialSettings.ShouldShow || _shown || _requested) return;
            Init();
            _requested = true;
            ShowAsync(_revision).Forget();
        }

        private async UniTask ShowAsync(int revision)
        {
            var text = await _localizedText.StringReference.GetLocalizedStringAsync();
            if (this == null || revision != _revision || !DialogueCompleted) return;

            if (_fade.isAlive) _fade.Stop();
            _group.alpha = 0f;
            _text.maxVisibleCharacters = int.MaxValue;
            gameObject.SetActive(true);
            _text.text = text;
            _text.ForceMeshUpdate();
            var characterCount = _text.textInfo.characterCount;
            var visibleCharacters = Mathf.Max(0, characterCount - trailingCharacters);
            _text.maxVisibleCharacters = visibleCharacters;
            _fade = Tween.Alpha(_group, endValue: 1f, duration: appearDuration, ease: Ease.InOutSine);
            _shown = true;
            _requested = false;

            // Keep typing throughout the fade so it is already underway when the hint becomes visible.
            while (visibleCharacters < characterCount)
            {
                _text.maxVisibleCharacters = ++visibleCharacters;
                await UniTask.Delay(System.TimeSpan.FromSeconds(characterDelay));
                if (this == null || revision != _revision) return;
            }
            _text.maxVisibleCharacters = int.MaxValue;
        }

        public void Hide(bool immediate = false)
        {
            Init();
            _revision++;
            _requested = false;
            if (_fade.isAlive) _fade.Stop();
            if (immediate || !gameObject.activeInHierarchy || _group.alpha <= 0f)
            {
                _group.alpha = 0f;
                gameObject.SetActive(false);
                return;
            }

            _fade = Tween.Alpha(_group, endValue: 0f, duration: fadeDuration, ease: Ease.InOutSine)
                .OnComplete(() => gameObject.SetActive(false));
        }

        public void ResetTip()
        {
            Hide(immediate: true);
            _shown = false;
        }

        private void OnDisable()
        {
            _revision++;
            _requested = false;
            if (_fade.isAlive) _fade.Stop();
        }
    }
}
