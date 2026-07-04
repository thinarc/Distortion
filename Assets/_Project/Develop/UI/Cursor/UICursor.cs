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
        }

        private void Update()
        {
            var target = _input.MousePosition;
            transform.position = Vector2.Lerp(transform.position, target, followSpeed * Time.unscaledDeltaTime);
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
            _cursorImage.sprite = (_mode, _state) switch
            {
                (CursorMode.Default, CursorState.Idle) => defaultIdle,
                (CursorMode.Default, CursorState.Click) => defaultClick,
                (CursorMode.Tool, CursorState.Idle) => toolIdle,
                (CursorMode.Tool, CursorState.Click) => toolClick,
                _ => defaultIdle
            };
        }

        private void AnimateState()
        {
            var targetScale = _state == CursorState.Click ? Vector3.one * clickScale : Vector3.one;
            Tween.Scale(transform, endValue: targetScale, duration: clickAnimTime, ease: Ease.OutBack, useUnscaledTime: true);
        }

        private void OnDisable()
        {
            _input.Dispose();
        }
        
        public enum CursorState
        {
            Idle,
            Click
        }
        
        public enum CursorMode
        {
            Default,
            Tool
        }
    }
}