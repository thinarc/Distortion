using Cysharp.Threading.Tasks;
using UnityEngine;
using _Project.Develop.Sound;

namespace _Project.Develop.Switch.Scenes

{
    public class DoorLocation : BaseLocation
    {
        public override async UniTask Show()
        {
            await base.Show();
            G.Get<SoundController>().MusicHandle.SetRadioPosition(new Vector3(-3f, 0f, -10f));
        }
    }
}