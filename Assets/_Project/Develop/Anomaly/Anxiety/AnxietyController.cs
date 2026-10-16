using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Develop.UI.HUD;
using PrimeTween;
using UnityEngine;

namespace _Project.Develop.Anomaly.Anxiety
{
    public class AnxietyController : IService, IDisposable
    {
        private readonly List<AnxietyModifier> _anxietyModifiers = new();
        private readonly List<AnxietySource> _anxietySources = new();

        private UIBar _bar;

        private bool _barWarned;
        
        private int _currentLevel = 1;

        public void Init()
        {
            G.Get<EventBus>().SceneReset += OnSceneReset;
        }
        
        public void Tick()
        {
            if (_anxietySources.Count == 0) return;
            
            var value = _anxietySources.Sum(source => source.EvaluateAnxiety);
            value = _anxietyModifiers.Aggregate(value, (current, modifier) => modifier.Modify(current));

            _bar ??= G.Get<UIBar>();
            _bar.AddValue(value * Time.deltaTime);
            
            var newLevel = ResolveLevel(_bar.Value);
            
            if (newLevel == _currentLevel)
                return;
            
            G.Get<EventBus>().AnxietyLevelChanged?.Invoke(_currentLevel, newLevel);
            _currentLevel = newLevel;
            
            var warn = _bar.Value > 0.75f;

            if (warn && !_barWarned && !_bar.Showing)
                _bar.Show(time: 0.45f, ease: Ease.InOutQuad, showPill: true);

            _barWarned = warn;
        }
        
        private void OnSceneReset()
        {
            _bar?.AddValue(-1 * 1000);
        }

        public void Apply(AnxietyModifier anxietyModifier)
        {
            _anxietyModifiers.Add(anxietyModifier);
        }

        public void Apply(AnxietySource anxietySource)
        {
            _anxietySources.Add(anxietySource);
        }

        public void Remove(AnxietyModifier anxietyModifier)
        {
            _anxietyModifiers.Remove(anxietyModifier);
        }

        public void Remove(AnxietySource anxietySource)
        {
            _anxietySources.Remove(anxietySource);
        }
        
        private static int ResolveLevel(float anxiety)
        {
            return anxiety switch
            {
                < 0.15f => 1,
                < 0.25f => 2,
                < 0.5f => 3,
                < 0.75f => 4,
                < 0.95f => 5,
                < 1f => 6,
                _ => 7
            };
        }
        
        public void Dispose()
        {
            G.Get<EventBus>().SceneReset -= OnSceneReset;
        }
    }
}