using _Project.Develop.Anomaly.Anxiety;
using _Project.Develop.Switch;
using _Project.Develop.UI.HUD;
using _Project.Develop.Sound;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPEffects.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace _Project.Develop.UI.Menus
{
    public class RestDialog : MonoBehaviour, IService
    {
        [SerializeField] private LocalizedString sleepTexts;
        [SerializeField] private LocalizedString breathTexts;
        [SerializeField] private LocalizedString chemicalTexts;
        
        private CanvasGroup _group;
        private TextMeshProUGUI _tmp;
        
        private SwitchModel _switchModel;
        private UIStats _stats;

        private bool _rest;
        private bool _disabled;
        
        public enum RestType
        {
            Sleep,
            Breath,
            Chemical
        }

        private void Start()
        {
            _group = GetComponent<CanvasGroup>();
            _tmp = GetComponentInChildren<TextMeshProUGUI>();

            G.Get<EventBus>().GoodEnd += OnSceneReset;
            G.Get<EventBus>().SceneReady += OnSceneReady;
        }
        
        public void Init(SwitchModel switchModel, UIStats stats)
        {
            _switchModel = switchModel;
            _stats = stats;
        }

        private void OnSceneReset()
        {
            _rest = false;
            _disabled = true;
        }

        private void OnSceneReady()
        {
            _disabled = false;
        }

        private void Update()
        {
            if (_rest)
                _stats.Timer += Time.deltaTime * 2f;
        }

        public async UniTaskVoid ApplyRest(float multiplier, RestType restType)
        {
            if (_rest || _disabled) return;

            var source = new AnxietySource(-15f);
            var modifier = new AnxietyModifier(multiplier);
            _rest = true;
            
            G.Get<EventBus>().EffectApply?.Invoke();
            
            var isStreetRest = restType == RestDialog.RestType.Breath;
            
            if (!isStreetRest) await G.Get<SoundController>().PauseForRest();
            
            await Tween.Alpha(_group, endValue: 1f, duration: 1f, ease: Ease.InOutSine);
            _group.blocksRaycasts = true;
            
            if (restType == RestType.Chemical)
            {
                G.Get<SoundController>().UIHandle.TakesPills();
            }
            else if (restType == RestType.Sleep)
            {
                G.Get<SoundController>().UIHandle.RestEnter(); 
            }
           

            var delay = restType switch
            {
                RestType.Sleep => Random.Range(800, 2000),
                RestType.Breath => Random.Range(500, 1200),
                RestType.Chemical => Random.Range(300, 1000),
                _ => 500
            };

            await UniTask.Delay(delay);

            var writer = _tmp.GetComponent<TMPWriter>();
            writer.DefaultDelays.SetDelay(Random.Range(0.1f, 0.2f));
            writer.DefaultDelays.SetPunctuationDelay(Random.Range(0.8f, 1.7f));
            
            var restText = restType switch
            {
                RestType.Sleep => sleepTexts.GetLocalizedString(),
                RestType.Breath => breathTexts.GetLocalizedString(),
                RestType.Chemical => chemicalTexts.GetLocalizedString(),
                _ => string.Empty
            };

            var texts = restText.Split('\n');

            _tmp.text = texts[Random.Range(0, texts.Length)];
            if (!Mathf.Approximately(_tmp.color.a, 1))
                _ = Tween.Alpha(_tmp, endValue: 1f, duration: 0);

            await _tmp.GetComponent<TMPWriter>().OnFinishWriter;

            await UniTask.Delay(Random.Range(1200, 2600));
            
            _ = _switchModel.GoToLocation("Workplace");
            source.Dispose();
            modifier.Dispose();
            
            if (!isStreetRest) _ = G.Get<SoundController>().ResumeForRest();
            
            _ = Tween.Alpha(_tmp, endValue: 0f, duration: 0.1f, ease: Ease.InOutSine);

            await Tween.Alpha(_group, endValue: 0f, duration: 0.4f, ease: Ease.InOutSine);
            _group.blocksRaycasts = false;
            
            _tmp.text = "";
            _rest = false;
            
            G.Get<EventBus>().EffectRemove?.Invoke();
        }

        private void OnDestroy()
        {
            G.Get<EventBus>().GoodEnd -= OnSceneReset;
            G.Get<EventBus>().SceneReady -= OnSceneReady;
        }
    }
}