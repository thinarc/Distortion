using _Project.Develop.Input;
using _Project.Develop.Sound;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Project.Develop.UI.Menus
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private CanvasGroup containerGroup;
        
        [SerializeField, Space(5)] private Slider volumeSlider;
        [SerializeField] private Image[] volumeButtons = new Image[4];
        
        [SerializeField, Space(5)] private CanvasGroup creditsGroup;
        [SerializeField] private GameObject creditsCol;
        [SerializeField] private Button creditsButton;
        
        [SerializeField, Space(5)] private CanvasGroup backgroundGroup;
        [SerializeField] private CanvasGroup lightGroup;
        [SerializeField] private TextMeshProUGUI startText;

        [SerializeField, Space(5)] private ParticleSystem rainParticles;
        
        private Button _startButton;
        private InputController _input;

        private float _quitTime = 2f;
        private float _pastVolume;

        private bool _quitHold;
        
        private bool _opened;
        private bool _started;
        private bool _credits;

        private bool _isPause;

        private void Start()
        {
            _startButton = startText.GetComponentInParent<Button>();
            _input = G.Get<InputController>();
            
            containerGroup.alpha = 0;
            containerGroup.interactable = false;
            containerGroup.blocksRaycasts = false;

            backgroundGroup.alpha = 0;
            lightGroup.alpha = 0;
            
            creditsGroup.alpha = 0;
            creditsGroup.interactable = false;
            creditsGroup.blocksRaycasts = false;
            creditsGroup.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            
            volumeSlider.value = G.Get<SoundController>().MasterVolume;
            ChooseVolumeHint();
            
            volumeSlider.onValueChanged.AddListener(_ => OnVolumeChange());
            
            _input.Player.Player.Info.performed += OnInfo;
            _input.Player.Player.Quit.performed += OnQuitDown;
            _input.Player.Player.Quit.canceled += OnQuitUp;
        }

        private void Update()
        {
            var input = G.Get<InputController>();
            
            if (_opened && !_credits && input.Player.Player.Navigate.ReadValue<Vector2>().x < 0) volumeSlider.value -= Time.unscaledDeltaTime * 0.8f;
            else if (_opened && !_credits && input.Player.Player.Navigate.ReadValue<Vector2>().x > 0) volumeSlider.value += Time.unscaledDeltaTime * 0.8f;
            
            if (_opened && _quitHold) _quitTime -= Time.unscaledDeltaTime;
            else _quitTime = Mathf.Clamp(_quitTime + Time.unscaledDeltaTime, 1f, 2f);
            
            if (_quitTime < 0f) QuitGame();
        }

        private void OnInfo(InputAction.CallbackContext ctx)
        {
            if (!_started && _opened && !_credits) ShowCredits();
            else if (!_started && _opened && _credits) CloseCredits();
        }
        
        private void OnQuitDown(InputAction.CallbackContext ctx)
        {
            if (_started && !_opened) ShowMenu(true);
            else if (_started && _opened) CloseMenu();
            else if (!_started && _opened && _credits) CloseCredits();
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
            Tween.StopAll(containerGroup);
            
            _ = Tween.Alpha(containerGroup, endValue: 1f, duration: 0.4f, startDelay: 0.1f, ease: Ease.OutSine, useUnscaledTime: true);
            containerGroup.interactable = true;
            containerGroup.blocksRaycasts = true;

            _isPause = pause;
            
            _ = Tween.Alpha(backgroundGroup, endValue: 1f, duration: 0.4f, ease: Ease.OutSine, useUnscaledTime: true);
            
            Time.timeScale = 0;
            G.Get<EventBus>().Pause?.Invoke();
            
            if (pause)
            {
                var main = rainParticles.main;
                main.prewarm = true;
                
                creditsButton.gameObject.SetActive(false);
                startText.text = "Вернуться";
                
                Tween.StopAll(lightGroup);
                
                _ = Tween.Alpha(lightGroup, endValue: 0.7f, duration: 1.2f, ease: Ease.OutSine, useUnscaledTime: true);
                G.Get<SoundController>().EnterPause();

                await UniTask.WaitForSeconds(0.2f, ignoreTimeScale: true);
            }
            else
            {
                _ = Tween.Alpha(lightGroup, endValue: 0.9f, duration: 1.4f, ease: Ease.OutSine, useUnscaledTime: true);
                G.Get<SoundController>().MusicHandle.PlayMainTheme();
            }
             
            rainParticles.gameObject.SetActive(true);
            _opened = true;
        }

        public async UniTask ShowMenu()
        {
            ShowMenu(false);
            await UniTask.WaitUntil(() => !_opened);
        }

        public void CloseMenu()
        {
            G.Get<SoundController>().UIHandle.PlayClick();
            Tween.StopAll(containerGroup);
            
            Tween.Alpha(containerGroup, endValue: 0f, duration: _isPause ? 0.2f : 0.6f, ease: Ease.InQuad, useUnscaledTime: true).OnComplete(() =>
            {
                Time.timeScale = 1f;
                G.Get<EventBus>().Resume?.Invoke();
                
                _opened = false;
                _started = true;
            });
            
            if (_isPause) G.Get<SoundController>().ExitPause();
            else G.Get<SoundController>().MusicHandle.PlayBgMusic();
            
            Tween.StopAll(lightGroup);

            Tween.Alpha(backgroundGroup, endValue: 0f, duration: _isPause ? 0.2f : 0.6f, startDelay: 0.1f, ease: Ease.InQuad, useUnscaledTime: true);
            Tween.Alpha(lightGroup, endValue: 0, duration: _isPause ? 0.2f : 0.4f, ease: Ease.InQuad, useUnscaledTime: true);
            
            containerGroup.interactable = false;
            containerGroup.blocksRaycasts = false;
            
            rainParticles.gameObject.SetActive(false);
        }
        
        public void ShowCredits()
        {
            Tween.Scale(creditsGroup.transform, endValue: 1, duration: 0.1f, ease: Ease.InOutSine, useUnscaledTime: true);
            Tween.Alpha(creditsGroup, endValue: 1, duration: 0.1f, ease: Ease.InOutSine, useUnscaledTime: true).OnComplete(() =>
            {
                creditsGroup.interactable = true;
                creditsGroup.blocksRaycasts = true;
                _credits = true;
                
                _startButton.interactable = false;
                containerGroup.GetComponent<UIEffects>().SlowMouseTilt();
            });
            creditsButton.interactable = false;
            creditsCol.SetActive(true);
            creditsButton.GetComponent<UIEffects>().OnPointerExit(null);
            
            G.Get<SoundController>().UIHandle.PlayClick();
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
            
            _startButton.interactable = true;
            containerGroup.GetComponent<UIEffects>().ReturnMouseTilt();
            
            G.Get<SoundController>().UIHandle.PlayExit();
        }
        
        public void OnVolumeChange()
        {
            G.Get<SoundController>().UIHandle.PlayScroll();
            ApplyVolume();
        }
          
        public void OnVolume()
        {
            if (_pastVolume < 0.2f) _pastVolume = 0.2f;
            volumeSlider.value = _pastVolume;
            ApplyVolume();
        }
        
        public void OffVolume()
        {
            _pastVolume = G.Get<SoundController>().MasterVolume;
            volumeSlider.value = 0f;
            ApplyVolume();
        }
        
        private void ApplyVolume()
        {
            G.Get<SoundController>().MasterVolume = volumeSlider.value;
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
            G.Get<SoundController>().UIHandle.PlayExit();
#endif
        }
    }
}