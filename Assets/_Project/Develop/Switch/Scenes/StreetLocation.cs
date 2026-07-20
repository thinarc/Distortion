using _Project.Develop.UI.Curtain;
using Cysharp.Threading.Tasks;
using PrimeTween;
using _Project.Develop.Sound;

namespace _Project.Develop.Switch.Scenes
{
    public class StreetLocation : BaseLocation
    {
        public override async UniTask Show()
        {
            G.Get<SoundController>().MusicHandle.SetWindow(1f);
            G.Get<SoundController>().AmbientHandle.PlayStreet();
            G.Get<UICurtain>().Show(TransitionTime, TransitionEase, toStreet: true);
            await base.Show();
        }

        public override async UniTask Hide(float duration, Ease ease)
        {
            G.Get<SoundController>().MusicHandle.SetWindow(0f);
            G.Get<SoundController>().AmbientHandle.StopStreet();
            G.Get<UICurtain>().Hide(duration, ease, fromStreet: true);
            await base.Hide(duration, ease);
        }
    }
}