using System;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace _Project.Develop.UI.Curtain
{
    public class UICurtain : IService, IDisposable
    {
        private readonly CurtainSide[] _sides;
        
        private readonly Light2D _globalLight;
        
        private readonly RectTransform _streetMask;
        private readonly Canvas _canvas;
        
        private CanvasGroup _group;
        
        private RawImage _streetImage;
        private Camera _cam;

        private Camera _baseCam;

        private bool _fullWidth;
        
        private bool _anyOpen;

        public bool AnyOpen
        {
            get => _anyOpen;
            private set
            {
                _anyOpen = value;
                G.Get<EventBus>().AnySidesChanged?.Invoke();
            }
        }
        
        private bool _bothOpen;

        public bool BothOpen
        {
            get => _bothOpen;
            private set
            {
                _bothOpen = value;
                G.Get<EventBus>().BothSidesChanged?.Invoke();
            }
        }

        public UICurtain(CurtainSide[] sides, Light2D globalLight, RectTransform streetMask, Canvas canvas)
        {
            _sides = sides;
            _globalLight = globalLight;
            _streetMask = streetMask;
            _canvas = canvas;
        }

        public void Init()
        {
            _group = _sides[0].GetComponentInParent<CanvasGroup>();
            _group.alpha = 0;
            _group.blocksRaycasts = false;

            _streetImage = _streetMask.GetComponentInChildren<RawImage>();
            _cam = _streetMask.GetComponentInChildren<Camera>(true);

            _baseCam = _canvas.worldCamera;
            
            SidesCycle().Forget();
            LightCycle().Forget();
            
            G.Get<EventBus>().SceneReset += OnSceneReset;
            G.Get<EventBus>().SceneReady += OnSceneReady;
            G.Get<EventBus>().SideDragging += OnSideDragging;
        }
        
        private async UniTaskVoid SidesCycle()
        {
            while (true)
            {
                if (!_anyOpen && _sides[0].Disclosure + _sides[1].Disclosure > 0.2f) AnyOpen = true;
                else if (_anyOpen && _sides[0].Disclosure + _sides[1].Disclosure < 0.2f) AnyOpen = false;
                
                if (!_bothOpen && (_sides[0].Disclosure > 0.1f && _sides[1].Disclosure > 0.1f)) BothOpen = true;
                else if (_bothOpen && (_sides[0].Disclosure <= 0.1f || _sides[1].Disclosure <= 0.1f)) BothOpen = false;
                
                await UniTask.Yield();
            }
        }

        private async UniTaskVoid LightCycle()
        {
            while (true)
            {
                var leftAdd = _sides[0].Disclosure / 10f;
                var rightAdd = _sides[1].Disclosure / 10f;

                var targetIntensity = 0.7f + leftAdd * 1.5f + rightAdd * 1.5f;

                _globalLight.intensity = Mathf.Lerp(_globalLight.intensity, targetIntensity, Time.deltaTime * 5f);
                await UniTask.Yield();
            }
        }

        private void OnSceneReset()
        {
            _fullWidth = true;
            SetMask(true);
        }

        private void OnSceneReady()
        {
            _fullWidth = false;
            OffMask(0f).Forget();
        }

        private void OnSideDragging()
        {
            SetMask(false);
        }

        public void Show(float time, Ease ease, bool toStreet)
        {
            Tween.Alpha(_group, endValue: 1f, duration: time, ease: ease);
            _group.blocksRaycasts = true;
            
            if (toStreet)
            {
                foreach (var side in _sides) side.Disable();
                Tween.Scale(_group.transform, endValue: 2.6f, duration: 0.01f);
            }
            
            Tween.Alpha(_streetImage, endValue: 1f, duration: time, ease: ease);
            SetMask(toStreet);
        }

        public void Hide(float time, Ease ease, bool fromStreet)
        {
            Tween.Alpha(_group, endValue: 0f, duration: time, ease: ease);
            _group.blocksRaycasts = false;
            
            Tween.Alpha(_streetImage, endValue: 0f, duration: time, ease: ease);
            OffMask(time).Forget();

            if (!fromStreet) return;
            foreach (var side in _sides) side.Enable();
            Tween.Scale(_group.transform, endValue: 1f, duration: 0.01f);
        }
        
        private void SetMask(bool inStreet)
        {
            var size = _streetMask.sizeDelta;
            size.x = _fullWidth ? 1920f : DisclosureWidth;
            size.y = inStreet ? 1080f : 865f;
            _streetMask.sizeDelta = inStreet ? size * 2.6f : size;
            
            var position = _streetMask.anchoredPosition;
            position.x = DisclosurePosition;
            _streetMask.anchoredPosition = inStreet ? position * 2.6f : position;
            
            _streetImage.rectTransform.localPosition = -_streetMask.localPosition;
            _cam.transform.localPosition = -_streetMask.localPosition;

            _canvas.worldCamera = _cam;
            _cam.gameObject.SetActive(true);
        }

        private async UniTaskVoid OffMask(float time)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(time));
            
            _canvas.worldCamera = _baseCam;
            _cam.gameObject.SetActive(false);
        }
        
        private float DisclosureWidth
        {
            get
            {
                var leftRect = _sides[0].GetComponent<RectTransform>();
                var rightRect = _sides[1].GetComponent<RectTransform>();

                var leftCorners = new Vector3[4];
                var rightCorners = new Vector3[4];

                leftRect.GetWorldCorners(leftCorners);
                rightRect.GetWorldCorners(rightCorners);

                var leftEdge = leftCorners[3];
                var rightEdge = rightCorners[0];

                var canvasRect = leftRect.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

                var leftLocal = canvasRect.InverseTransformPoint(leftEdge);
                var rightLocal = canvasRect.InverseTransformPoint(rightEdge);
                
                return Mathf.Max(rightLocal.x - leftLocal.x, 0) + 50;
            }
        }

        private float DisclosurePosition
        {
            get
            {
                var leftCorners = new Vector3[4];
                var rightCorners = new Vector3[4];

                _sides[0].GetComponent<RectTransform>().GetWorldCorners(leftCorners);
                _sides[1].GetComponent<RectTransform>().GetWorldCorners(rightCorners);

                var parent = _streetMask.parent as RectTransform;

                var left = parent.InverseTransformPoint(leftCorners[3]);
                var right = parent.InverseTransformPoint(rightCorners[0]);

                return (left.x + right.x) * 0.5f;
            }
        }

        public void Dispose()
        {
            G.Get<EventBus>().SceneReset -= OnSceneReset;
            G.Get<EventBus>().SceneReady -= OnSceneReady;
            G.Get<EventBus>().SideDragging -= OnSideDragging;
        }
    }
}