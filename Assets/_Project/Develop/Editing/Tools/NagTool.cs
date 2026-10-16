using UnityEngine.InputSystem;
using _Project.Develop.Sound;
using _Project.Develop.Editing.Photo;

namespace _Project.Develop.Editing.Tools
{
    public class NagTool : PickupTool
    {
        private bool _notEffective;
        
        protected override void Start()
        {
            base.Start();
            
            G.Get<EventBus>().StartEditing += OnStartEditing;
            G.Get<EventBus>().EditingAno += OnEditingAno;
        }

        private void OnStartEditing() => _notEffective = false;
        private void OnEditingAno() => _notEffective = true;

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
        
        protected override void OnSpotEnter(EditingSpot spot)
        {
            G.Get<SoundController>().ActionHandle.PlayNag();
            if (!_notEffective) base.OnSpotEnter(spot);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            G.Get<EventBus>().StartEditing -= OnStartEditing;
            G.Get<EventBus>().EditingAno -= OnEditingAno;
        }
    }
}