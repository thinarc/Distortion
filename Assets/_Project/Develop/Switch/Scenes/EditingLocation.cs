using _Project.Develop.Anomaly;
using _Project.Develop.Anomaly.Anxiety;
using _Project.Develop.Editing;
using _Project.Develop.Sound;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.Utilities;
using UnityEngine;

namespace _Project.Develop.Switch.Scenes
{
    public class EditingLocation : BaseLocation
    {
        [SerializeField, Space(5)] private float negativeAnxiety = 1f;
        
        private AnxietySource _locationSource;

        private EditView _view;

        public override void Init()
        {
            base.Init();
            _view = GetComponentInChildren<EditView>();
        }
        
        public override async UniTask Show()
        {
            await base.Show();
            _locationSource = new AnxietySource(negativeAnxiety);
            
            G.Get<SoundController>().MusicHandle.SetRadioPosition(new Vector3(0f, 0f, -9.7f));
        }
        
        protected override void ActivateSources() => CurrentAnomalies.ForEach(ano =>
        {
            if (ano.applyType != ApplyType.Mouse) return;
            ano.ActivateSourceVoid();
            ano.PlaySound(look: true);
        });

        public override async UniTask Hide(float duration, Ease ease)
        {
            _locationSource.Dispose();
            await base.Hide(duration, ease);
        }
        
        protected override void DeactivateSources() => CurrentAnomalies.ForEach(ano =>
        {
            if (ano.applyType != ApplyType.Mouse) return;
            ano.DeactivateSource().Forget();
            ano.StopSound(look: true);
        });

        protected override void OnApply(AnomalyObject ano)
        {
            if (ano.applyType == ApplyType.Command)
                _view.AddAnomaly(ano, discardLocation: this);
            else if (ano.applyType == ApplyType.Mouse)
                ano.Apply();
            else throw new System.NotImplementedException();
        }

        protected override void OnRemove(AnomalyObject ano)
        {
            if (ano.applyType == ApplyType.Command)
                _view.DiscardAnomaly(ano);
            else if (ano.applyType == ApplyType.Mouse)
                ano.Remove();
            else throw new System.NotImplementedException();
        }
    }
}