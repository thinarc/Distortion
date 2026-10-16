using _Project.Develop.Anomaly;
using _Project.Develop.UI.Curtain;
using _Project.Develop.Sound;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace _Project.Develop.Switch.Scenes
{
    public class WindowLocation : BaseLocation
    {
        [SerializeField, Space(5)] private Image streetPic;
        [SerializeField] private Sprite sky2;
        
        [SerializeField, Space(5)] private Light2D streetLight;
        
        private Sprite _sky1;
        private UICurtain _curtain;

        private CanvasGroup _streetGroup;

        private bool _showing;
        
        public override void Init()
        {
            base.Init();
            
            _sky1 = streetPic.sprite;
            _curtain = G.Get<UICurtain>();
            
            _streetGroup = streetPic.GetComponentInParent<CanvasGroup>(true);

            G.Get<EventBus>().AnySidesChanged += OnAnySidesChanged;
        }
        
        public override async UniTask Show()
        {
            _streetGroup.gameObject.SetActive(true);
            _ = Tween.Alpha(_streetGroup, endValue: 1f, duration: TransitionTime, ease: TransitionEase);
            
            G.Get<UICurtain>().Show(TransitionTime, TransitionEase, toStreet: false);
            await base.Show();
            
            _showing = true;
            
            G.Get<SoundController>().MusicHandle.SetRadioPosition(new Vector3(3f, 0f, -10f));

            while (_showing)
            {
                await UniTask.Delay(200);
                
                var sprite = streetPic.sprite == sky2 ? _sky1 : sky2;
                streetPic.sprite = sprite;
            }
        }
        
        protected override void ActivateSources() => CurrentAnomalies.ForEach(ano => ano.PlaySound(look: true));

        public override async UniTask Hide(float duration, Ease ease)
        {
            _ = Tween.Alpha(_streetGroup, endValue: 0f, duration: TransitionTime, ease: TransitionEase).OnComplete(() =>
                _streetGroup.gameObject.SetActive(false));
            
            G.Get<UICurtain>().Hide(duration, ease, fromStreet: false);
            await base.Hide(duration, ease);
            
            _showing = false;
        }
        
        protected override void DeactivateSources() => CurrentAnomalies.ForEach(ano => ano.PlaySound(look: false));
        
        public override async UniTaskVoid UpdateLight(bool initial, Light2D forcedLight = null)
        {
            if (initial) await UniTask.Delay(200);
            base.UpdateLight(initial, streetLight).Forget();
        }
        
        protected override void OnApply(AnomalyObject ano)
        {
            if (ano.applyType != ApplyType.Animation) base.OnApply(ano);
            else ano.Apply(sky2, _sky1);
            if (_curtain.AnyOpen)
            {
                ano.ActivateSourceVoid();
                ano.PlaySound(curtainOpen: _curtain.AnyOpen);
            }
        }
        
        protected override void OnRemove(AnomalyObject ano)
        {
            if (ano.applyType != ApplyType.Animation) base.OnRemove(ano);
            else ano.Remove(sky2, _sky1);
            ano.StopSound(curtainOpen: _curtain.AnyOpen);
        }
        
        private void OnAnySidesChanged()
        {
            if (_curtain.AnyOpen)
                foreach (var ano in CurrentAnomalies)
                {
                    ano.ActivateSourceVoid();
                    ano.PlaySound(curtainOpen: _curtain.AnyOpen);
                }
            else
                foreach (var ano in CurrentAnomalies)
                {
                    ano.DeactivateSource().Forget();
                    ano.StopSound(curtainOpen: _curtain.AnyOpen);
                }
        }

        private void OnDestroy()
        {
            G.Get<EventBus>().AnySidesChanged -= OnAnySidesChanged;
        }
    }
}