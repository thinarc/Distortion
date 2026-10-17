using System;
using System.ComponentModel;
using _Project.Develop.Sound;
using _Project.Develop.UI.HUD;
using UnityEngine;

namespace _Project.Develop.Anomaly
{
    public class AnomalyController : IService, IDisposable
    {
        private readonly AnomalyView _anomalyView;

        public AnomalyController(AnomalyView anomalyView)
        {
            _anomalyView = anomalyView;
        }

        public void Init()
        {
            G.Get<EventBus>().AnxietyLevelChanged += OnAnxietyLevelChanged;
        }

        private void OnAnxietyLevelChanged(int previousLevel, int currentLevel)
        {
            if (currentLevel > previousLevel)
                for (var level = previousLevel + 1; level <= currentLevel; level++)
                    Upgrade(level);
            else
                for (var level = previousLevel; level > currentLevel; level--)
                    Downgrade(level);
        }
        
        private void Upgrade(int level)
        {
            switch (level)
            {
                case 1:
                    throw new IndexOutOfRangeException();
                case 2:
                    _anomalyView.SpawnOnStreet();
                    break;
                case 3:
                    _anomalyView.SpawnInRoom();
                    break;
                case 4:
                    _anomalyView.SpawnOnStreet();
                    _anomalyView.SpawnInRoom();
                    G.Get<SoundController>().MusicHandle.SetEvil(1f);
                    break;
                case 5:
                    _anomalyView.SpawnInRoom();
                    break;
                case 6:
                    G.Get<SoundController>().MusicHandle.SetRumble(1f);
                    break;
                case 7:
                    G.Get<EventBus>().BadEnd?.Invoke();
                    break;
            }
        }

        private void Downgrade(int level)
        {
            switch (level)
            {
                case 1:
                    _anomalyView.RemoveAll();
                    break;
                case 2:
                    _anomalyView.RemoveAllFromRoom();
                    break;
                case 3:
                    _anomalyView.RemoveAllFromStreet();
                    G.Get<SoundController>().MusicHandle.SetEvil(0f);
                    break;
                case 4:
                    _anomalyView.RemoveFromRoom();
                    break;
                case 5:
                    _anomalyView.RemoveFromRoom();
                    G.Get<SoundController>().MusicHandle.SetRumble(0f);
                    break;
                case 6:
                case 7:
                    Debug.LogWarning("Downgrade from ending?" + $" {G.Get<UIBar>().Value}");
                    break;
            }
        }

        public void Dispose()
        {
            G.Get<EventBus>().AnxietyLevelChanged -= OnAnxietyLevelChanged;
        }
    }
}