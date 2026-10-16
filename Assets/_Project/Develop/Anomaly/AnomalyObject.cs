using System;
using System.Threading;
using _Project.Develop.Anomaly.Anxiety;
using _Project.Develop.Input;
using _Project.Develop.Sound;
using _Project.Develop.Sound.Handles;
using static _Project.Develop.Sound.Handles.AmbientHandle;
using _Project.Develop.Switch.Scenes;
using _Project.Develop.UI;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Project.Develop.Anomaly
{
    [Serializable]
    public class AnomalyObject
    {
        public ApplyType applyType;
        [ShowIf(nameof(NotSpawnToyType))] public Image target;
        [ShowIf(nameof(SpawnToyType))] public Image[] targetVariants;
        
        [ShowIf(nameof(DefaultType))] public Sprite sprite;
        [ShowIf(nameof(AnimationType))] public Sprite[] frames;

        [SerializeField] private float anxiety = 1f;
        [SerializeField] private float lifetime;

        [SerializeField, Space(5)] private AnomalySound sound;
        [SerializeField] private AnomalySoundRule soundRule;
        
        [ShowIf(nameof(MouseType)), SerializeField] private float mouseJitterStrength = 5f;
        [ShowIf(nameof(MouseType)), SerializeField] private float mouseDriftSpeed = 2f;
        
        private CancellationTokenSource _mouseTrackCts;
        
        private bool DefaultType => applyType == ApplyType.Default;
        private bool AnimationType => applyType == ApplyType.Animation;
        private bool SpawnToyType => applyType == ApplyType.SpawnToy;
        private bool NotSpawnToyType => applyType != ApplyType.SpawnToy;
        private bool MouseType => applyType == ApplyType.Mouse;
        
        private AnxietySource _anomalySource;
        
        private float _currentLifetime;
        private bool _applied;

        private bool _soundApplied;

        public bool AppliedSource => _anomalySource != null;
        
        public void Apply(params Sprite[] currentFrames)
        {
            switch (applyType)
            {
                case ApplyType.Default:
                    (target.sprite, sprite) = (sprite, target.sprite);
                    break;
                case ApplyType.Spawn:
                    target.gameObject.SetActive(true);
                    break;
                case ApplyType.Animation:
                    if (currentFrames == null) throw new NullReferenceException();
                    (currentFrames[0], currentFrames[1]) = (frames[0], frames[1]);
                    break;
                case ApplyType.SpawnToy:
                    targetVariants.ForEach(t => t.gameObject.SetActive(false));
                    targetVariants[Random.Range(0, targetVariants.Length)].gameObject.SetActive(true);
                    if (AppliedSource) break;
                    if (anxiety != 0f) _anomalySource = new AnxietySource(anxiety);
                    break;
                case ApplyType.Mouse:
                    if (anxiety != 0f) _anomalySource = new AnxietySource(anxiety);
                    break;
            }
            
            _currentLifetime = lifetime;
            _applied = true;
            
            if (soundRule.streetState != StreetState.None)
                G.Get<SoundController>().AmbientHandle.SetStreetState(soundRule.streetState, active: true);

            
            PlaySound();
        }
        
        public void ActivateSourceVoid()
        {
            _ = ActivateSource();
        }

        public UniTask ActivateSourceTask()
        {
            return ActivateSource();
        }

        private async UniTask ActivateSource()
        {
            if (applyType == ApplyType.Mouse)
            {
                _mouseTrackCts?.Cancel();
                _mouseTrackCts = new CancellationTokenSource();
                InfluenceMouseLoop(_mouseTrackCts.Token).Forget();
                return;
            }
            
            if (AppliedSource) return;
            if (applyType == ApplyType.SpawnToy) return;
            
            if (anxiety != 0f) _anomalySource = new AnxietySource(anxiety);

            if (applyType is ApplyType.Spawn or ApplyType.Command)
            {
                await G.Get<DarknessEffect>().ApplyEffect();
                target.GetComponentInParent<BaseLocation>().RemoveAnomaly(this);
            }
            
            if (lifetime == 0f || !_applied) return;

            while (_currentLifetime < lifetime && AppliedSource && _applied)
            {
                _currentLifetime += Time.deltaTime / 1.6f;
                await UniTask.Yield();
            }
        }
        
        public void PlaySound(bool look = false, bool curtainOpen = false)
        {
            // Debug.Log($"PlaySound called: look={look}, curtainOpen={curtainOpen}");
            if (sound == AnomalySound.None) return;
            if (soundRule.playOnLook != look) return;
            if (soundRule.requiresCurtainOpen != curtainOpen) return;

            if (soundRule.stopOnLook == look && _soundApplied)
            {
                StopSound(look, curtainOpen);
                return;
            }
            
            G.Get<SoundController>().AnomalyHandle.Play(sound);
            _soundApplied = true;
        }
        
        public void Remove(params Sprite[] currentFrames)
        {
            switch (applyType)
            {
                case ApplyType.Default:
                    (target.sprite, sprite) = (sprite, target.sprite);
                    break;
                case ApplyType.Spawn:
                    target.gameObject.SetActive(false);
                    break;
                case ApplyType.Animation:
                    if (currentFrames == null) throw new NullReferenceException();
                    (frames[0], frames[1]) = (currentFrames[0], currentFrames[1]);
                    break;
                case ApplyType.SpawnToy:
                    targetVariants.ForEach(t => t.gameObject.SetActive(false));
                    _anomalySource?.Dispose();
                    _anomalySource = null;
                    break;
                case ApplyType.Mouse:
                    _anomalySource?.Dispose();
                    _anomalySource = null;
                    break;
            }
            
            _applied = false;
            DeactivateSource().Forget();
            
            if (soundRule.streetState != StreetState.None)
                G.Get<SoundController>().AmbientHandle.SetStreetState(soundRule.streetState, active: false);
            
            StopSound();
        }

        public async UniTaskVoid DeactivateSource()
        {
            if (applyType == ApplyType.Mouse)
            {
                _mouseTrackCts?.Cancel();
                _mouseTrackCts?.Dispose();
                _mouseTrackCts = null;
                return;
            }
            
            if (applyType != ApplyType.SpawnToy)
            {
                _anomalySource?.Dispose();
                _anomalySource = null;
            }
            
            if (lifetime == 0f || !_applied) return;

            while (_currentLifetime > 0f && AppliedSource && _applied)
            {
                _currentLifetime -= Time.deltaTime;
                await UniTask.Yield();
            }
            
            target.GetComponentInParent<BaseLocation>(true).RemoveAnomaly(this);
        }

        public void StopSound(bool look = false, bool curtainOpen = false)
        {
            if (sound == AnomalySound.None) return;
            if (soundRule.playOnLook != look) return;
            if (soundRule.requiresCurtainOpen != curtainOpen) return;
            
            G.Get<SoundController>().AnomalyHandle.Stop(sound);
            _soundApplied = false;
        }
        
        private async UniTaskVoid InfluenceMouseLoop(CancellationToken token)
        {
            var driftDirection = Random.insideUnitCircle.normalized; 
            var timeCounter = 0f;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    timeCounter += Time.deltaTime;
                    
                    if (timeCounter > 3f)
                    {
                        driftDirection = Random.insideUnitCircle.normalized;
                        timeCounter = 0f;
                    }
                    
                    var jitter = new Vector2(
                        Random.Range(-mouseJitterStrength, mouseJitterStrength),
                        Random.Range(-mouseJitterStrength, mouseJitterStrength)
                    );
                    
                    var drift = driftDirection * mouseDriftSpeed;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var currentPos = GetCursorPosition();
            SetCursorPosition(currentPos.x + (int)(jitter.x + drift.x), currentPos.y - (int)(jitter.y + drift.y));
#else
                    UnityEngine.InputSystem.Mouse.current.WarpCursorPosition(UnityEngine.InputSystem.Mouse.current.position.ReadValue() + jitter + drift);
#endif

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException) { }
        }
        
#if UNITY_STANDALONE_WIN
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);

        private struct Point { public int X; public int Y; }

        private static void SetCursorPosition(int x, int y) => SetCursorPos(x, y);
        private static Vector2Int GetCursorPosition() 
        {
            if (GetCursorPos(out var p)) return new Vector2Int(p.X, p.Y);
            return Vector2Int.zero;
        }
#endif
    }

    public enum ApplyType
    {
        Default = 0,
        Spawn = 1,
        Animation = 2,
        Command = 3,
        SpawnToy = 4,
        Mouse = 5
    }
    
    [Serializable]
    public class AnomalySoundRule
    {
        [ShowIf(nameof(NotStopOnLook))] public bool playOnLook;
        [ShowIf(nameof(NotPlayOnLook))] public bool stopOnLook;
        public bool requiresCurtainOpen;

        public StreetState streetState = StreetState.None;
        
        private bool NotPlayOnLook => !playOnLook;
        private bool NotStopOnLook => !stopOnLook;
    }
}