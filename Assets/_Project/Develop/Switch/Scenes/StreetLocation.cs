using System.Linq;
using _Project.Develop.Anomaly;
using _Project.Develop.Anomaly.Anxiety;
using _Project.Develop.UI.Curtain;
using Cysharp.Threading.Tasks;
using PrimeTween;
using _Project.Develop.Sound;
using _Project.Develop.UI;
using _Project.Develop.UI.Menus;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace _Project.Develop.Switch.Scenes
{
    public class StreetLocation : BaseLocation
    {
        [SerializeField, Space(5)] private UIEffects curtainTilt;
        [SerializeField, Space(5)] private UIEffects[] streetTilts;
        
        [SerializeField, Space(5)] private Image pic;
        [SerializeField] private Sprite sky2;

        [SerializeField, Space(5)] private float positiveAnxiety = 1f;
        private AnxietySource _positiveSource;
        
        private Sprite _sky1;
        private UICurtain _curtain;

        private float _restTimer;
        
        private bool _showing;

        public override void Init()
        {
            base.Init();
            
            _sky1 = pic.sprite;
            _curtain = G.Get<UICurtain>();

            G.Get<EventBus>().AnySidesChanged += OnAnySidesChanged;
        }

        public override async UniTask Show()
        {
            G.Get<SoundController>().MusicHandle.SetWindow(1f);
            G.Get<SoundController>().AmbientHandle.PlayStreet();
            G.Get<SoundController>().AnomalyHandle.SetPlayerOnStreet(true);
            
            G.Get<UICurtain>().Show(TransitionTime, TransitionEase, toStreet: true);
            await base.Show();

            curtainTilt.enabled = true;
            streetTilts.ForEach(t => t.enabled = true);
            
            if (CurrentAnomalies.Count == 0)
                _positiveSource = new AnxietySource(-positiveAnxiety);

            _restTimer = 6f;
            
            _showing = true;

            while (_showing)
            {
                await UniTask.Delay(200);
                
                var sprite = pic.sprite == sky2 ? _sky1 : sky2;
                pic.sprite = sprite;
                
                if (_positiveSource != null && CurrentAnomalies.Count > 0)
                {
                    _positiveSource.Dispose();
                    _positiveSource = null;
                }
                else _restTimer -= 0.2f;

                if (_restTimer < 0)
                {
                    G.Get<RestDialog>().ApplyRest(2f, RestDialog.RestType.Breath).Forget();
                    //G.Get<RestDialog>().ApplyRest(2f, Random.value < 0.5f ? RestDialog.RestType.Breath : RestDialog.RestType.Sleep).Forget();
                    _restTimer = 100f;
                }
            }
            
            _positiveSource?.Dispose();
            _positiveSource = null;
        }

        protected override void ActivateSources() => CurrentAnomalies.ForEach(ano => ano.PlaySound(look: true));

        public override async UniTask Hide(float duration, Ease ease)
        {
            G.Get<SoundController>().MusicHandle.SetWindow(0f);
            G.Get<SoundController>().AmbientHandle.StopStreet();
            G.Get<SoundController>().AnomalyHandle.SetPlayerOnStreet(false);
            
            G.Get<UICurtain>().Hide(duration, ease, fromStreet: true);
            await base.Hide(duration, ease);
            
            curtainTilt.enabled = false;
            streetTilts.ForEach(t => t.enabled = false);
            
            _showing = false;
        }
        
        protected override void DeactivateSources() => CurrentAnomalies.ForEach(ano => ano.PlaySound(look: false));
        
        public override async UniTaskVoid UpdateLight(bool initial, Light2D forcedLight = null)
        {
            if (initial) await UniTask.Delay(100);
            base.UpdateLight(initial, forcedLight).Forget();
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
                foreach (var ano in CurrentAnomalies.ToList())
                {
                    ano.ActivateSourceVoid();
                    ano.PlaySound(curtainOpen: _curtain.AnyOpen);
                }
            else
                foreach (var ano in CurrentAnomalies.ToList())
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