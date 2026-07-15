using _Project.Develop.UI.Curtain;
using Cysharp.Threading.Tasks;
using PrimeTween;

namespace _Project.Develop.Switch.Scenes
{
    public class StreetLocation : BaseLocation
    {
        public override async UniTask Show()
        {
            G.Get<UICurtain>().Show(TransitionTime, TransitionEase, toStreet: true);
            await base.Show();
        }

        public override async UniTask Hide(float duration, Ease ease)
        {
            G.Get<UICurtain>().Hide(duration, ease, fromStreet: true);
            await base.Hide(duration, ease);
        }
    }
}