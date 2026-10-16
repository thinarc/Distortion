using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace _Project.Develop.Sound.Handles
{
    public class AmbientHandle : IDisposable
    {
        // ===== FIELDS =====

        private EventInstance _street;
        private EventInstance _room;

        // Лейбл для FMOD-параметра "StreetStatement" на каждое состояние улицы
        private static readonly Dictionary<StreetState, string> StreetStateLabels = new()
        {
            { StreetState.Calm, "Calm" },
            { StreetState.AcidRain, "AcidRain" },
            { StreetState.TheHole, "theHole" }
        };

        // Счётчик активных источников на каждое "не-Calm" состояние.
        // Нужен, чтобы уборка одной аномалии не гасила вторую, если обе активны разом.
        private readonly Dictionary<StreetState, int> _streetStateCounts = new()
        {
            { StreetState.AcidRain, 0 },
            { StreetState.TheHole, 0 }
        };

        // Порядок = приоритет: если активны оба состояния — слышен первое по списку
        private static readonly StreetState[] StatePriority = { StreetState.AcidRain, StreetState.TheHole };


        // ===== INIT / DISPOSE =====

        public void Init()
        {
            _street = RuntimeManager.CreateInstance("event:/bg/Street");
            _room = RuntimeManager.CreateInstance("event:/bg/Room Ambience");
        }

        // Обязательный метод для очистки памяти FMOD (вызывается один раз при закрытии приложения)
        public void Dispose()
        {
            if (_street.isValid())
            {
                _street.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _street.release();
            }

            if (_room.isValid())
            {
                _room.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _room.release();
            }
        }


        // ===== STREET =====

        public void PlayStreet()
        {
            _street.start();
        }

        public void StopStreet()
        {
            _street.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }


        // ===== ROOM AMBIENCE =====

        public void PlayAmbience()
        {
            _room.start();
        }

        public void StopAmbience()
        {
            _room.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }


        // ===== STREET STATE (аномальные состояния улицы: AcidRain, TheHole...) =====

        public void SetStreetState(StreetState state, bool active)
        {
            if (state == StreetState.None || state == StreetState.Calm) return;

            _streetStateCounts[state] += active ? 1 : -1;
            ApplyStreetState();
        }

        // Сброс на дефолт — например, на SceneReset, чтобы следующая ночь не наследовала счётчики прошлой
        public void ResetStreetState()
        {
            _streetStateCounts[StreetState.AcidRain] = 0;
            _streetStateCounts[StreetState.TheHole] = 0;
            ApplyStreetState();
        }

        private void ApplyStreetState()
        {
            foreach (var state in StatePriority)
            {
                if (_streetStateCounts[state] > 0)
                {
                    _street.setParameterByNameWithLabel("StreetStatement", StreetStateLabels[state]);
                    return;
                }
            }

            _street.setParameterByNameWithLabel("StreetStatement", StreetStateLabels[StreetState.Calm]);
        }


        public enum StreetState
        {
            None = 0,
            Calm = 1,
            AcidRain = 2,
            TheHole = 3
        }
    }
}