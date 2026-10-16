using System;
using _Project.Develop.Input;
using _Project.Develop.Switch;
using _Project.Develop.UI.Cursor;
using _Project.Develop.Sound;
using _Project.Develop.Sound.Handles;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPEffects.Components;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Project.Develop.UI.Menus
{
    public class EndingMenu : MonoBehaviour, IService
    {
        [SerializeField] private Image blackScreen;
        [SerializeField, Space(5)] private CanvasGroup buttonsGroup;
        [SerializeField] private Button retryButton;
        
        [SerializeField, Space(5)] private TextMeshProUGUI nightsCounter;
        [SerializeField] private TextMeshProUGUI clickText;

        [SerializeField, Space(5)] private Image[] goodPics;

        [SerializeField, Space(5)] private CanvasGroup averageGroup;
        [SerializeField] private CanvasGroup dialogGroup;
        
        private TextMeshProUGUI _dialogText;
        private Button _teleButton;

        [SerializeField, Space(5)] private CanvasGroup badGroup;
        
        private Animator _badAnim;
        private SwitchModel _switchModel;
        
        [SerializeField, Space(5)] private LanguageSelector langSelector;

        [SerializeField, Space(5)] private CanvasGroup quitGroup;
        
        public void Init()
        {
            _dialogText = dialogGroup.GetComponentInChildren<TextMeshProUGUI>();
            _teleButton = averageGroup.GetComponentInChildren<Button>();
            
            _badAnim = badGroup.GetComponent<Animator>();
            
            langSelector.Init();
            
            var eventBus = G.Get<EventBus>();
            
            eventBus.PassNight += OnPassNight;
            eventBus.GoodEnd += OnGoodEnd;
            eventBus.AverageEnd += OnAverageEnd;
            eventBus.BadEnd += OnBadEnd;
            
            DontDestroyOnLoad(this);
        }

        public void Init2(SwitchModel switchModel)
        {
            _switchModel = switchModel;
        }

        public async UniTask SelectLanguage()
        {
            _ = Tween.Alpha(blackScreen, endValue: 1f, duration: 0.4f);
            blackScreen.raycastTarget = true;
            
            await langSelector.Show(duration: 0.4f, startDelay: 0.2f);
            
            _ = Tween.Alpha(blackScreen, endValue: 0f, duration: 0.4f);
            await langSelector.Hide(duration: 0.3f);
            
            blackScreen.raycastTarget = false;
        }

        private void OnPassNight(int completedNights)
        {
            OnPassNightAsync(completedNights).Forget();
        }

        private async UniTaskVoid OnPassNightAsync(int completedNights)
        {
            G.Get<EventBus>().Pause?.Invoke();
            G.Get<UICursor>().Hide();
            
            await UniTask.Delay(100);
            
            _ = Tween.Alpha(blackScreen, endValue: 1f, duration: 0.4f);
            blackScreen.raycastTarget = true;
            
            await Tween.Alpha(nightsCounter, endValue: 1f, duration: 0.2f, startDelay: 0.2f);

            await UniTask.Delay(240);
            G.Get<EventBus>().SceneReset?.Invoke();
            
            await UniTask.Delay(500);

            var index = nightsCounter.text.LastIndexOf(' ');
            nightsCounter.text = $"{nightsCounter.text[..index]} {completedNights + 1}";
            
            await UniTask.Delay(700);
            
            clickText.text = ". . .";

            await clickText.GetComponent<TMPWriter>().OnFinishWriter;
            await UniTask.Delay(140);
            
            G.Get<UICursor>().Show();
            
            G.Get<InputController>().Player.Player.Click.performed += OnClick;
            return;

            void OnClick(InputAction.CallbackContext ctx)
            {
                OnClickAsync().Forget();
            }

            async UniTaskVoid OnClickAsync()
            {
                _ = Tween.Alpha(nightsCounter, endValue: 0f, duration: 0.2f);
                _ = Tween.Alpha(blackScreen, endValue: 0f, duration: 0.4f);
                
                _ = Tween.Alpha(clickText, endValue: 0f, duration: 0.2f).OnComplete(() =>
                {
                    clickText.text = "";
                });
                
                _switchModel.GoToLocation("Workplace").Forget();
                G.Get<EventBus>().SceneReady?.Invoke();
                
                G.Get<InputController>().Player.Player.Click.performed -= OnClick;

                await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
                blackScreen.raycastTarget = false;
            }
        }

        private void OnGoodEnd()
        {
            OnGoodEndAsync().Forget();
        }

        private async UniTaskVoid OnGoodEndAsync()
        {
            G.Get<SoundController>().SetEndingState(true);
            G.Get<EventBus>().Pause?.Invoke();
            G.Get<UICursor>().Hide();
            G.Get<SoundController>().MusicHandle.StopMusic(); //Стоп сразу как сцена кончилась.Синхрон
            
            await _switchModel.GoToLocation("Street");
            
            G.Get<InputController>().Player.Player.MousePosition.performed += OnMousePosition;
            return;

            void OnMousePosition(InputAction.CallbackContext ctx)
            {
                OnMousePositionAsync().Forget();
            }

            async UniTaskVoid OnMousePositionAsync()
            {
                G.Get<InputController>().Player.Player.MousePosition.performed -= OnMousePosition;
                
                G.Get<EventBus>().SceneReset?.Invoke();
                await UniTask.Delay(300);
            
                //G.Get<SoundController>().EndingHandle.PlayEndingStinger(EndingType.ImDone);
                // расскоментируй если звук нужен раньше картинки
            
                await Tween.Alpha(goodPics[0], endValue: 1f, duration: 0.7f, ease: Ease.InOutSine);
            
                G.Get<SoundController>().EndingHandle.PlayEndingStinger(EndingType.ImDone);
                //соответственно тогда убери это
            
                await UniTask.Delay(1200);
            
                await Tween.Alpha(goodPics[1], endValue: 1f, duration: 2.7f, ease: Ease.InOutSine);
            
                await UniTask.Delay(1200);
            
                _ = Tween.Alpha(blackScreen, endValue: 1f, duration: 0.4f);
                blackScreen.raycastTarget = true;
            
                await Tween.Alpha(buttonsGroup, endValue: 1f, duration: 0.3f, startDelay: 0.1f);
            
                G.Get<UICursor>().Show();
            
                _switchModel.GoToLocation("Workplace").Forget();
                G.Get<EventBus>().SceneReady?.Invoke();

                buttonsGroup.blocksRaycasts = true;
                buttonsGroup.interactable = true;
            }
        }

        private void OnAverageEnd()
        {
            OnAverageEndAsync().Forget();
        }

        private async UniTaskVoid OnAverageEndAsync()
        {
            G.Get<SoundController>().MusicHandle.StopMusic();
            G.Get<SoundController>().SetEndingState(true);
            G.Get<EventBus>().Pause?.Invoke();
            G.Get<UICursor>().Hide();
            
            await _switchModel.GoToLocation("Workplace");
            await UniTask.Delay(300);
            
            await Tween.Alpha(averageGroup, endValue: 1f, duration: 0.7f);
            
            G.Get<SoundController>().UIHandle.PhoneRings();
            
            G.Get<EventBus>().SceneReset?.Invoke();
            
            await UniTask.Delay(900);
            
            G.Get<UICursor>().Show();
            await UniTask.Delay(540);
            
            _teleButton.onClick.AddListener(TakePhone);
            G.Get<EventBus>().SceneReady?.Invoke();
            
            averageGroup.blocksRaycasts = true;
            averageGroup.interactable = true;
        }

        private void TakePhone()
        {
            TakePhoneAsync().Forget();
        }

        private async UniTaskVoid TakePhoneAsync()
        {
            G.Get<SoundController>().UIHandle.StopPhoneRings();
            
            _teleButton.onClick.RemoveListener(TakePhone);
            averageGroup.blocksRaycasts = false;
            averageGroup.interactable = false;
            
            var phrase = _dialogText.text;
            _dialogText.text = "";
            
            _ = Tween.Alpha(dialogGroup, endValue: 1f, duration: 0.4f, ease: Ease.InOutSine);
            await UniTask.Delay(400);
            
            _dialogText.text = "...";
            
            await UniTask.Delay(820);
            _dialogText.text = "";
            
            await UniTask.Delay(170);
            _dialogText.text = "...";
            
            await _dialogText.GetComponent<TMPWriter>().OnFinishWriter;
            await UniTask.Delay(1000);

            _dialogText.text = phrase;
                
            await _dialogText.GetComponent<TMPWriter>().OnFinishWriter;
            await UniTask.Delay(1400);
                
            _ = Tween.Alpha(blackScreen, endValue: 1f, duration: 0.4f);
            blackScreen.raycastTarget = true;
            
            await Tween.Alpha(buttonsGroup, endValue: 1f, duration: 0.3f, startDelay: 0.1f);

            buttonsGroup.blocksRaycasts = true;
            buttonsGroup.interactable = true;
        }

        private void OnBadEnd()
        {
            OnBadEndAsync().Forget();
        }

        private async UniTaskVoid OnBadEndAsync()
        {
            G.Get<EventBus>().Pause?.Invoke();
            G.Get<UICursor>().Hide();
            G.Get<SoundController>().MusicHandle.StopMusic(); //Стоп сразу как сцена кончилась.Синхрон
            G.Get<SoundController>().SetEndingState(true);
            
            await _switchModel.GoToLocation("Workplace");
            await UniTask.Delay(200);
            
            await Tween.Alpha(badGroup, endValue: 1f, duration: 0.7f);
            G.Get<EventBus>().SceneReset?.Invoke();
            
            await UniTask.Delay(900);
            
            _badAnim.SetTrigger("Show");
            G.Get<SoundController>().EndingHandle.PlayEndingStinger(EndingType.ImAnomaly); //НЕ УДАЛЯЙ. Синхрон звука и анимции
            await UniTask.Yield();

            var duration = TimeSpan.FromSeconds(_badAnim.GetCurrentAnimatorStateInfo(0).length);
            
            await UniTask.Delay(duration / 2);
            Time.timeScale = 0f;

            G.Get<InputController>().Player.Player.MousePosition.performed += OnMousePosition;
            return;

            void OnMousePosition(InputAction.CallbackContext ctx)
            {
                OnMousePositionAsync().Forget();
            }

            async UniTaskVoid OnMousePositionAsync()
            {
                G.Get<InputController>().Player.Player.MousePosition.performed -= OnMousePosition;
                Time.timeScale = 1f;
                
                await UniTask.Delay(duration - duration / 2);
                await UniTask.Delay(1000);
                
                _ = Tween.Alpha(blackScreen, endValue: 1f, duration: 0.4f);
                blackScreen.raycastTarget = true;
                
                await Tween.Alpha(buttonsGroup, endValue: 1f, duration: 0.3f, startDelay: 0.1f);
            
                G.Get<UICursor>().Show();
                G.Get<EventBus>().SceneReady?.Invoke();
                
                buttonsGroup.blocksRaycasts = true;
                buttonsGroup.interactable = true;
            }
        }

        public void RetryGame()
        {
            RetryGameAsync().Forget();
        }

        private async UniTaskVoid RetryGameAsync()
        {
            G.Get<SoundController>().SetEndingState(false);
            G.Get<SoundController>().EndingHandle.StopStinger();
            
            buttonsGroup.blocksRaycasts = false;
            buttonsGroup.interactable = false;
            
            _ = Tween.Alpha(goodPics[0], endValue: 0f, duration: 0.3f);
            _ = Tween.Alpha(goodPics[1], endValue: 0f, duration: 0.3f);
            
            _ = Tween.Alpha(averageGroup, endValue: 0f, duration: 0.3f);
            _ = Tween.Alpha(dialogGroup, endValue: 0f, duration: 0.3f);
            
            _ = Tween.Alpha(badGroup, endValue: 0f, duration: 0.3f);
                
            _ = Tween.Alpha(blackScreen, endValue: 0f, duration: 0.4f);
            await Tween.Alpha(buttonsGroup, endValue: 0f, duration: 0.3f);
            
            blackScreen.raycastTarget = false;
            
            G.Get<EventBus>().SceneReady?.Invoke();
        }

        public void QuitGame()
        {
            QuitGameAsync().Forget();
        }
        
        private async UniTaskVoid QuitGameAsync()
        {
            G.Get<SoundController>().SetEndingState(false);
            
            _ = Tween.Scale(quitGroup.transform, endValue: 1.1f, duration: 1.0f, ease: Ease.OutCubic);
            await Tween.Alpha(quitGroup, endValue: 1f, duration: 0.8f, ease: Ease.OutQuad);
            
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void OnDestroy()
        {
            var eventBus = G.Get<EventBus>();
            
            eventBus.PassNight -= OnPassNight;
            eventBus.GoodEnd -= OnGoodEnd;
            eventBus.AverageEnd -= OnAverageEnd;
            eventBus.BadEnd -= OnBadEnd;
        }
    }
}