using System;
using System.Collections.Generic;
using _Project.Develop.Configs;
using _Project.Develop.Input;
using _Project.Develop.Switch.Scenes;
using _Project.Develop.UI.Curtain;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Project.Develop.Switch
{
    public class SwitchView : MonoBehaviour, IDisposable
    {
        [SerializeField] private List<Button> buttons;
        
        private CanvasGroup _group;

        private void Start()
        {
            _group = GetComponent<CanvasGroup>();
        }
        
        public void BindButtons(SwitchController controller)
        {
            buttons[0].onClick.AddListener(controller.MoveLeft);
            buttons[1].onClick.AddListener(controller.MoveRight);
            buttons[2].onClick.AddListener(controller.MoveBottom);
            buttons[3].onClick.AddListener(controller.MoveUp);
            buttons[4].onClick.AddListener(controller.MoveThings);
            
            G.Get<InputController>().Player.Player.Navigate.performed += OnNavigate;
            G.Get<EventBus>().CurtainChanged += OnCurtainChanged;
            
            Tween.Alpha(_group, endValue: 1f, duration: 0.4f, Ease.InOutSine);
        }
        
        private void OnNavigate(InputAction.CallbackContext ctx)
        {
            var direction = ctx.ReadValue<Vector2>();
            if (direction.x < 0 && buttons[0].interactable) buttons[0].onClick.Invoke();
            else if (direction.x > 0 && buttons[1].interactable) buttons[1].onClick.Invoke();
            else if (direction.y < 0 && buttons[2].interactable) buttons[2].onClick.Invoke();
            else if (direction.y > 0 && buttons[3].interactable) buttons[3].onClick.Invoke();
            else if (direction.y > 0 && buttons[4].interactable) buttons[4].onClick.Invoke();
        }
        
        public async UniTask ChangeLocation(BaseLocation current, BaseLocation target)
        {
            InteractButtons(new DirectionsData { left = "", right = "", bottom = "", up = "", things = "" });
            
            if (current != null)
            {
                current.UpdateLight(initial: false);
                await current.Hide(current.TransitionTime, current.TransitionEase);
            }

            target.UpdateLight(initial: target.Config.key == "Window" || G.Get<UICurtain>().BothOpen);
            await target.Show();
            
            InteractButtons(target.Config.directions);
        }
        
        private void InteractButtons(DirectionsData directions)
        {
            buttons[0].interactable = !string.IsNullOrEmpty(directions.left);
            buttons[1].interactable = !string.IsNullOrEmpty(directions.right);
            buttons[2].interactable = !string.IsNullOrEmpty(directions.bottom);
            buttons[3].interactable = !string.IsNullOrEmpty(directions.up) && G.Get<UICurtain>().BothOpen;
            buttons[4].interactable = !string.IsNullOrEmpty(directions.things) && G.Get<UICurtain>().BothOpen;

            if (!buttons[3].interactable && !buttons[2].interactable) return;
            buttons[2].gameObject.SetActive(!buttons[3].interactable);
            buttons[3].gameObject.SetActive(buttons[3].interactable);
        }

        private void OnCurtainChanged()
        {
            buttons[3].interactable = G.Get<UICurtain>().BothOpen;
            buttons[4].interactable = G.Get<UICurtain>().BothOpen;
        }

        public void Dispose()
        {
            G.Get<EventBus>().CurtainChanged -= OnCurtainChanged;
        }
    }
}