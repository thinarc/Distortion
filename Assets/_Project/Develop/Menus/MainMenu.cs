using _Project.Develop.Input;
using _Project.Develop.Sound;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Project.Develop.Menus
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Image[] volumeButtons = new Image[4];
        
        [SerializeField, Space(5)] private CanvasGroup creditsGroup;
        [SerializeField] private GameObject creditsCol;
        [SerializeField] private Button creditsButton;
        
        [SerializeField, Space(5)] private CanvasGroup lightGroup;
        [SerializeField] private TextMeshProUGUI startText;

        [SerializeField, Space(5)] private ParticleSystem rainParticle;
        
        private CanvasGroup _group;
        private InputController _input;

        private float _quitTime = 2f;
        private float _pastVolume;

        private bool _quitHold;
        
        private bool _open;
        private bool _started;
        private bool _credits;

        private void Start()
        {
            _group = GetComponentInChildren<CanvasGroup>();
            _input = G.Get<InputController>();
            
            _group.alpha = 0;
            _group.interactable = false;
            _group.blocksRaycasts = false;
            
            lightGroup.alpha = 0;
            
            creditsGroup.alpha = 0;
            creditsGroup.interactable = false;
            creditsGroup.blocksRaycasts = false;
            
            volumeSlider.value = G.Get<SoundController>().MasterVolume;
            ChooseVolumeHint();
            
            _input.Player.Player.Info.performed += OnInfo;
            _input.Player.Player.Quit.performed += OnQuitDown;
            _input.Player.Player.Quit.canceled += OnQuitUp;
        }

        private void Update()
        {
            var input = G.Get<InputController>();
            
            if (_open && !_credits && input.Player.Player.Navigate.ReadValue<Vector2>().x < 0) volumeSlider.value -= Time.unscaledDeltaTime * 0.8f;
            else if (_open && !_credits && input.Player.Player.Navigate.ReadValue<Vector2>().x > 0) volumeSlider.value += Time.unscaledDeltaTime * 0.8f;
            
            if (_open && _quitHold) _quitTime -= Time.unscaledDeltaTime;
            else _quitTime = Mathf.Clamp(_quitTime + Time.unscaledDeltaTime, 1f, 2f);
            
            if (_quitTime < 0f) QuitGame();
        }

        private void OnInfo(InputAction.CallbackContext ctx)
        {
            if (!_started && _open && !_credits) ShowCredits();
            else if (!_started && _open && _credits) CloseCredits();
        }
        
        private void OnQuitDown(InputAction.CallbackContext ctx)
        {
            if (_started && !_open) ShowMenu(true);
            else if (_started && _open) CloseMenu(true);
            else if (!_started && _open && _credits) CloseCredits();
            _quitHold = true;
        }
        
        private void OnQuitUp(InputAction.CallbackContext ctx)
        {
            _quitHold = false;
        }

        private void OnDisable()
        {
            var input = G.Get<InputController>();
            input.Player.Player.Info.performed -= OnInfo;
            _input.Player.Player.Quit.performed -= OnQuitDown;
            _input.Player.Player.Quit.canceled -= OnQuitUp;
        }
        
        public async void ShowMenu(bool pause)
        {
            _ = Tween.Alpha(_group, endValue: 1f, duration: 0.4f, ease: Ease.InOutSine, useUnscaledTime: true);
            _group.interactable = true;
            _group.blocksRaycasts = true;
            
            Time.timeScale = 0;
            _open = true;
            
            if (pause)
            {
                var main = rainParticle.main;
                main.prewarm = true;
                
                creditsButton.gameObject.SetActive(false);
                startText.text = "Вернуться";

                await UniTask.WaitForSeconds(0.2f, ignoreTimeScale: true);
            }
            else _ = Tween.Alpha(lightGroup, endValue: 1f, duration: 1.4f, ease: Ease.InOutSine, useUnscaledTime: true);
             
            rainParticle.gameObject.SetActive(true);
        }

        public async UniTask ShowMenu()
        {
            ShowMenu(false);
            await UniTask.WaitUntil(() => !_open);
        }

        public void CloseMenu(bool pause = false)
        {
            _ = Tween.Alpha(_group, endValue: 0f, duration: pause ? 0.2f : 0.6f, ease: Ease.InOutSine, useUnscaledTime: true).OnComplete(() =>
            {
                Time.timeScale = 1f;
                _open = false;
                _started = true;
            });
            
            _group.interactable = false;
            _group.blocksRaycasts = false;
            
            if (!pause) Tween.Alpha(lightGroup, endValue: 0, duration: 0.4f, ease: Ease.InOutSine, useUnscaledTime: true);
            
            rainParticle.gameObject.SetActive(false);
        }
        
        public void ShowCredits()
        {
            Tween.Scale(creditsGroup.transform, endValue: 1, duration: 0.1f, ease: Ease.InOutSine, useUnscaledTime: true);
            Tween.Alpha(creditsGroup, endValue: 1, duration: 0.1f, ease: Ease.InOutSine, useUnscaledTime: true).OnComplete(() =>
            {
                creditsGroup.interactable = true;
                creditsGroup.blocksRaycasts = true;
                _credits = true;
            });
            creditsButton.interactable = false;
            creditsCol.SetActive(true);
        }
        
        public void CloseCredits()
        {
            Tween.Scale(creditsGroup.transform, endValue: 0.8f, duration: 0.2f, ease: Ease.InOutSine, useUnscaledTime: true);
            Tween.Alpha(creditsGroup, endValue: 0, duration: 0.08f, ease: Ease.InOutSine, useUnscaledTime: true).OnComplete(() =>
            {
                creditsButton.interactable = true;
                _credits = false;
            });
            creditsGroup.interactable = false;
            creditsGroup.blocksRaycasts = false;
            creditsCol.SetActive(false);
            
        }
        
        public void OnVolumeChange()
        {
            G.Get<SoundController>().MasterVolume = volumeSlider.value;
            ChooseVolumeHint();
        }
          
        public void OnVolume()
        {
            if (_pastVolume < 0.2f) _pastVolume = 0.2f;
            volumeSlider.value = _pastVolume;
            OnVolumeChange();
        }
        
        public void OffVolume()
        {
            _pastVolume = G.Get<SoundController>().MasterVolume;
            G.Get<SoundController>().MasterVolume = 0f;
            volumeSlider.value = 0f;
            ChooseVolumeHint();
        }
        
        private void ChooseVolumeHint()
        {
            volumeButtons.ForEach(h => h.gameObject.SetActive(false));
            if (volumeSlider.value < 0.05f) volumeButtons[0].gameObject.SetActive(true);
            else if (volumeSlider.value < 0.4f) volumeButtons[1].gameObject.SetActive(true);
            else if (volumeSlider.value < 0.9f) volumeButtons[2].gameObject.SetActive(true);
            else if (volumeSlider.value <= 1f) volumeButtons[3].gameObject.SetActive(true);
        }
        
        public void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}