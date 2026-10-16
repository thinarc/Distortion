using System.Collections.Generic;
using System.Linq;
using _Project.Develop.Anomaly;
using _Project.Develop.Configs;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace _Project.Develop.Switch.Scenes
{
    public abstract class BaseLocation : MonoBehaviour
    {
        [SerializeField] private LocationConfig config;

        [SerializeField, Space(5)] private float transitionTime = 0.2f;
        [SerializeField] private Ease transitionEase = Ease.Linear;

        [SerializeField, Space(5)] private List<AnomalyObject> anomalies;

        [SerializeField, Space(5)] private bool slowEv;
        
        public LocationConfig Config => config;
        
        public float TransitionTime => transitionTime;
        public Ease TransitionEase => transitionEase;
        
        private CanvasGroup _group;
        private Light2D _light;

        private readonly List<AnomalyObject> _currentAnomalies = new();
        private readonly List<AnomalyObject> _tempAnomalies = new();
        
        protected IReadOnlyList<AnomalyObject> CurrentAnomalies => _currentAnomalies;

        private float _initialLight;
        private bool _lightUpdateToInitial;

        private bool _showed;
        
        public virtual void Init()
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
            _group.interactable = true;
            ActivateSources();
        }

        protected virtual void ActivateSources() => _currentAnomalies.ForEach(ano =>
        {
            ano.ActivateSourceVoid();
            ano.PlaySound(look: true);
        });

        public virtual async UniTask Hide(float duration, Ease ease)
        {
            _group.blocksRaycasts = false;
            _group.interactable = false;
            await Tween.Alpha(_group, endValue: 0f, duration: duration, ease: ease);
            gameObject.SetActive(false);
            DeactivateSources();
        }
        
        protected virtual void DeactivateSources() => _currentAnomalies.ForEach(ano =>
        {
            ano.DeactivateSource().Forget();
            ano.StopSound(look: true);
        });

        public virtual async UniTaskVoid UpdateLight(bool initial, Light2D forcedLight = null)
        {
            _lightUpdateToInitial = initial;
            
            var target = _initialLight;
            var ev = 12f;
            
            var l = forcedLight ?? _light;
            if (slowEv) ev /= 8f;
            
            if (!initial)
            {
                target = 0;
                ev = 20f;
            }
            
            while (true)
            {
                l.intensity = Mathf.MoveTowards(l.intensity, target, Time.deltaTime * ev);
                await UniTask.Yield();
                if (Mathf.Approximately(l.intensity, target) || _lightUpdateToInitial != initial) break;
            }
        }

        public bool ApplyAnomaly()
        {
            if (anomalies.Count == 0) return false;
            
            var ano = anomalies[Random.Range(0, anomalies.Count)];
            _currentAnomalies.Add(ano);
            anomalies.Remove(ano);
            OnApply(ano);

            if (_group.blocksRaycasts)
            {
                ano.ActivateSourceVoid();
                ano.PlaySound(look: true);
            }

            _tempAnomalies.AddRange(anomalies.Where(a => a.target == ano.target));
            anomalies.RemoveAll(a => a.target == ano.target);
            
            return true;
        }
        
        protected virtual void OnApply(AnomalyObject ano) => ano.Apply();
        
        public bool RemoveAnomaly()
        {
            if (_currentAnomalies.Count == 0) return false;
            
            var ano = anomalies[Random.Range(0, anomalies.Count)];
            RemoveAnomaly(ano);
            
            return true;
        }
        
        public void RemoveAllAnomaly()
        {
            while(_currentAnomalies.Count > 0)
                RemoveAnomaly(_currentAnomalies[^1]);
        }

        public void RemoveAnomaly(AnomalyObject ano)
        {
            if (!_currentAnomalies.Remove(ano)) return;
            anomalies.Add(ano);
            OnRemove(ano);
                
            anomalies.AddRange(_tempAnomalies.Where(a => a.target == ano.target));
            _tempAnomalies.RemoveAll(a => a.target == ano.target);
        }
        
        protected virtual void OnRemove(AnomalyObject ano) => ano.Remove();
    }
}