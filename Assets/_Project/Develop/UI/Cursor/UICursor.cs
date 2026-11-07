using System.Collections.Generic;
using _Project.Develop.Anomaly;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.UI.Cursor
{
    public class UICursor : MonoBehaviour, IService
    {
        [SerializeField] private float followSpeed = 25f;
        [SerializeField] private float clickScale = 0.85f;
        [SerializeField] private float clickAnimTime = 0.08f;
        
        [SerializeField, Space(5)] private Sprite defaultIdle;
        [SerializeField] private Sprite defaultClick;
        [SerializeField] private Sprite toolIdle;
        [SerializeField] private Sprite toolClick;
        [SerializeField] private Sprite anomalySprite;

        private readonly HashSet<AnomalyObject> _activeMouseAnomalies = new();
        
        private Image _cursorImage;

        private CursorState _state;
        private CursorMode _mode;

        private CursorInput _input;

        private void Start()
        {
            _cursorImage = GetComponentInChildren<Image>();
            
            _input = new CursorInput(this);
            _input.Init();
            
            SetState(CursorState.Idle);
            GetComponent<CanvasGroup>().alpha = 1;
            
            UnityEngine.Cursor.lockState = CursorLockMode.Confined;
            UnityEngine.Cursor.visible = false;

            G.Get<EventBus>().SceneReset += OnSceneReset;
            G.Get<EventBus>().Pause += OnPause;
            G.Get<EventBus>().Resume += OnResume;
        }

        private void Update()
        {
            var target = _input.MousePosition;
            transform.position = Vector2.Lerp(transform.position, target, followSpeed * Time.unscaledDeltaTime);
        }

        public void Hide()
        {
            Tween.Alpha(_cursorImage, endValue: 0f, duration: 0.2f, ease: Ease.InOutSine);
        }
        
        public void Show()
        {
            Tween.Alpha(_cursorImage, endValue: 1f, duration: 0.4f, ease: Ease.InOutSine);
        }
        
        public void SetDefaultMode()
        {
            SetMode(CursorMode.Default);
        }

        public void SetToolMode()
        {
            SetMode(CursorMode.Tool);
        }

        public void SetState(CursorState newState)
        {
            if (_state == newState) return;

            _state = newState;
            UpdateSprite();
            AnimateState();
        }
        
        private void SetMode(CursorMode newMode)
        {
            _mode = newMode;
            UpdateSprite();
        }

        private void UpdateSprite()
        {
            _cursorImage.sprite = _activeMouseAnomalies.Count > 0 && anomalySprite != null
                ? anomalySprite
                : (_mode, _state) switch
            {
                (CursorMode.Default, CursorState.Idle) => defaultIdle,
                (CursorMode.Default, CursorState.Click) => defaultClick,
                (CursorMode.Tool, CursorState.Idle) => toolIdle,
                (CursorMode.Tool, CursorState.Click) => toolClick,
                _ => defaultIdle
            };

            _cursorImage.rectTransform.anchoredPosition = _activeMouseAnomalies.Count > 0 && anomalySprite != null
                ? new Vector2(25f, -60f)
                : _mode switch
            {
                CursorMode.Tool => new Vector2(0f, -10f),
                _ => new Vector2(40f, -60f)
            };
        }

        public void SetMouseAnomalyActive(AnomalyObject anomaly, bool active)
        {
            if (active) _activeMouseAnomalies.Add(anomaly);
            else _activeMouseAnomalies.Remove(anomaly);

            if (_cursorImage != null) UpdateSprite();
        }

        private void AnimateState()
        {
            var targetScale = _state == CursorState.Click ? Vector3.one * clickScale : Vector3.one;
            Tween.Scale(transform, endValue: targetScale, duration: clickAnimTime, ease: Ease.OutBack, useUnscaledTime: true);
        }
        
        private CursorMode _pastMode;
        
        private void OnSceneReset()
        {
            _activeMouseAnomalies.Clear();
            _pastMode = CursorMode.Default;
            _state = CursorState.Idle;
            Tween.StopAll(transform);
            transform.localScale = Vector3.one;
            SetDefaultMode();
        }

        private void OnPause()
        {
            _pastMode = _mode;
            SetDefaultMode();
        }
        
        private void OnResume()
        {
            _mode = _pastMode;
            UpdateSprite();
        }

        private void OnDisable()
        {
            _input.Dispose();
        }

        private void OnDestroy()
        {
            G.Get<EventBus>().SceneReset -= OnSceneReset;
            G.Get<EventBus>().Pause -= OnPause;
            G.Get<EventBus>().Resume -= OnResume;
        }
        
        public enum CursorState
        {
            Idle,
            Click
        }
        
        private enum CursorMode
        {
            Default,
            Tool
        }
    }
}
