using System.Collections.Generic;
using _Project.Develop.Configs;
using _Project.Develop.Input;
using _Project.Develop.Switch.Scenes;
using _Project.Develop.UI.Curtain;
using _Project.Develop.UI.HUD;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Project.Develop.Switch
{
    public class SwitchView : MonoBehaviour
    {
        [SerializeField] private List<Button> buttons;
        
        private CanvasGroup _group;
        
        private bool _init;

        private void Start()
        {
            _group = GetComponent<CanvasGroup>();
        }
        
        public void Init(SwitchController controller)
        {
            buttons[0].onClick.AddListener(controller.MoveLeft);
            buttons[1].onClick.AddListener(controller.MoveRight);
            buttons[2].onClick.AddListener(controller.MoveBottom);
            buttons[3].onClick.AddListener(controller.MoveUp);
            buttons[4].onClick.AddListener(controller.MoveThings);
            
            G.Get<InputController>().Player.Player.Navigate.performed += OnNavigate;
            
            var eventBus = G.Get<EventBus>();
            
            eventBus.BothSidesChanged += OnBothSidesChanged;
            eventBus.Pause += OnPause;
            eventBus.Resume += OnResume;
            eventBus.EffectApply += OnPause;
            eventBus.EffectRemove += OnResume;
            eventBus.SceneReset += OnSceneReset;
        }
        
        private void OnBothSidesChanged()
        {
            buttons[3].interactable = G.Get<UICurtain>().BothOpen;
            buttons[4].interactable = G.Get<UICurtain>().BothOpen;
        }

        private void OnPause()
        {
            if (_group.alpha != 0) Tween.Alpha(_group, endValue: 0f, duration: 0.3f, Ease.InOutSine, useUnscaledTime: true);
            _group.interactable = false;
        }

        private void OnResume()
        {
            Tween.Alpha(_group, endValue: 1f, duration: 0.4f, Ease.InOutSine).OnComplete(() => _group.interactable = true);
        }

        private void OnSceneReset()
        {
            _init = false;
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
            InteractButtons(null);
            
            if (current != null)
            {
                if (current.Config.key != "Editing") G.Get<UIBar>().Hide(current.TransitionTime, current.TransitionEase);
                
                current.UpdateLight(initial: false).Forget();
                await current.Hide(current.TransitionTime, current.TransitionEase);
            }

            target.UpdateLight(initial: target.Config.key == "Window" || G.Get<UICurtain>().BothOpen).Forget();
            if (_init) _ = target.Show();
            else
            {
                await target.Show();
                _init = true;
            }
            
            if (target.Config.key != "Editing") G.Get<UIBar>().Show(target.TransitionTime, target.TransitionEase);
            
            InteractButtons(target.Config.directions);
        }
        
        private void InteractButtons(DirectionsData directions)
        {
            if (directions == null)
            {
                OnPause();
                return;
            }
            
            buttons[0].gameObject.SetActive(!string.IsNullOrEmpty(directions.left));
            buttons[1].gameObject.SetActive(!string.IsNullOrEmpty(directions.right));
            buttons[2].gameObject.SetActive(!string.IsNullOrEmpty(directions.bottom));
            buttons[3].gameObject.SetActive(!string.IsNullOrEmpty(directions.up));
            
            OnResume();
            OnBothSidesChanged();
        }

        private void OnDestroy()
        {
            var eventBus = G.Get<EventBus>();
            
            eventBus.BothSidesChanged -= OnBothSidesChanged;
            eventBus.Pause -= OnPause;
            eventBus.Resume -= OnResume;
            eventBus.EffectApply -= OnPause;
            eventBus.EffectRemove -= OnResume;
            eventBus.SceneReset -= OnSceneReset;
        }
    }
}