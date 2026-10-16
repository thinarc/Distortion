using UnityEngine.InputSystem;
using _Project.Develop.Sound;
using _Project.Develop.Editing.Photo;

namespace _Project.Develop.Editing.Tools
{
    public class BrushTool : PickupTool
    {
        protected override void PickUp()
        {
            base.PickUp();
            G.Get<EventBus>().DustEnter += OnSpotEnter;
        }

        protected override void Drop(InputAction.CallbackContext obj)
        {
            base.Drop(obj);
            G.Get<EventBus>().DustEnter -= OnSpotEnter;
        }

        protected override void OnSpotEnter(EditingSpot spot)
        {
            G.Get<SoundController>().ActionHandle.PlayBrush();
            base.OnSpotEnter(spot);
        }
    }
}