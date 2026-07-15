using _Project.Develop.Configs;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace _Project.Develop.Switch.Scenes
{
    public abstract class BaseLocation : MonoBehaviour
    {
        [FormerlySerializedAs("directions")] [SerializeField] private LocationConfig config;

        [SerializeField, Space(5)] private float transitionTime = 0.2f;
        [FormerlySerializedAs("transition")] [SerializeField] private Ease transitionEase = Ease.Linear;
        
        public LocationConfig Config => config;
        
        public float TransitionTime => transitionTime;
        public Ease TransitionEase => transitionEase;

        private CanvasGroup _group;
        private Light2D _light;

        private float _initialLight;
        private bool _lightUpdateToInitial;

        public void Initialize()
        {
            _group = GetComponent<CanvasGroup>();
            _group.alpha = 0;
            
            _light = GetComponentInChildren<Light2D>();
            _initialLight = _light.intensity;
            _light.intensity = 0;
        }

        public virtual async UniTask Show()
        {
            gameObject.SetActive(true);
            await Tween.Alpha(_group, endValue: 1f, duration: transitionTime, ease: transitionEase);
            _group.blocksRaycasts = true;
        }

        public virtual async UniTask Hide(float duration, Ease ease)
        {
            _group.blocksRaycasts = false;
            await Tween.Alpha(_group, endValue: 0f, duration: duration, ease: ease);
            gameObject.SetActive(false);
        }

        public async void UpdateLight(bool initial)
        {
            _lightUpdateToInitial = initial;
                
            while (true)
            {
                var target = _initialLight;
                var ev = 12f;
            
                if (!initial)
                {
                    target = 0;
                    ev = 20f;
                }
                
                _light.intensity = Mathf.MoveTowards(_light.intensity, target, Time.deltaTime * ev);
                await UniTask.Yield();
                if (Mathf.Approximately(_light.intensity, target) || _lightUpdateToInitial != initial) break;
            }
        }
    }
}