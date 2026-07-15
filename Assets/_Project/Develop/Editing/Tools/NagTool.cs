using UnityEngine.InputSystem;

namespace _Project.Develop.Editing.Tools
{
    public class NagTool : PickupTool
    {
        protected override void PickUp()
        {
            base.PickUp();
            G.Get<EventBus>().DirtEnter += OnSpotEnter;
        }
        
        protected override void Drop(InputAction.CallbackContext obj)
        {
            base.Drop(obj);
            G.Get<EventBus>().DirtEnter -= OnSpotEnter;
        }
    }
}