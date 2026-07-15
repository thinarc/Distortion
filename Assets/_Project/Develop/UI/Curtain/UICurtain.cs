using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace _Project.Develop.UI.Curtain
{
    public class UICurtain : IService
    {
        private readonly CurtainSide[] _sides;
        
        private readonly Light2D _globalLight;
        
        private CanvasGroup _group;

        private bool _bothOpen;

        public bool BothOpen
        {
            // get => _bothOpen;
            get => true;
            private set
            {
                _bothOpen = value;
                G.Get<EventBus>().CurtainChanged?.Invoke();
            }
        }

        public UICurtain(CurtainSide[] sides, Light2D globalLight)
        {
            _sides = sides;
            _globalLight = globalLight;
        }

        public void Initialize()
        {
            _group = _sides[0].GetComponentInParent<CanvasGroup>();
            _group.alpha = 0;
            _group.blocksRaycasts = false;
            
            SidesCycle();
            LightCycle();
        }
        
        private async void SidesCycle()
        {
            while (true)
            {
                if (!_bothOpen && (_sides[0].Disclosure > 0.1f || _sides[1].Disclosure > 0.1f)) BothOpen = true;
                else if (_bothOpen && (_sides[0].Disclosure <= 0.1f && _sides[1].Disclosure <= 0.1f)) BothOpen = false;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        private async void LightCycle()
        {
            while (true)
            {
                var leftAdd = _sides[0].Disclosure / 10f;
                var rightAdd = _sides[1].Disclosure / 10f;

                var targetIntensity = 0.7f + leftAdd * 1.5f + rightAdd * 1.5f;

                _globalLight.intensity = Mathf.Lerp(_globalLight.intensity, targetIntensity, Time.deltaTime * 5f);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        public void Show(float time, Ease ease, bool toStreet)
        {
            Tween.Alpha(_group, endValue: 1f, duration: time, ease: ease);
            _group.blocksRaycasts = true;

            if (!toStreet) return;
            foreach (var side in _sides) side.Disable();
            Tween.Scale(_group.transform, endValue: 2.6f, duration: 0.01f);
        }

        public void Hide(float time, Ease ease, bool fromStreet)
        {
            Tween.Alpha(_group, endValue: 0f, duration: time, ease: ease);
            _group.blocksRaycasts = false;

            if (!fromStreet) return;
            foreach (var side in _sides) side.Enable();
            Tween.Scale(_group.transform, endValue: 1f, duration: 0.01f);
        }
    }
}