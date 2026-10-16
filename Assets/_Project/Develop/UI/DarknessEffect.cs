using _Project.Develop.Input;
using _Project.Develop.Sound;
using _Project.Develop.UI.Cursor;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Project.Develop.UI
{
    public class DarknessEffect : MonoBehaviour, IService
    {
        [SerializeField] private Canvas canvas;
        
        private RawImage _effect;
        
        private Camera _screenCam;
        private Camera _baseCam;
        
        private TextMeshProUGUI _spaceText;
        private CanvasGroup _group;
        
        private float _brightness;
        private float _gaussianBlur;
        private float _alphaDissolve;
        
        private void Start()
        {
            _effect = GetComponentInChildren<RawImage>();

            _screenCam = GetComponentInChildren<Camera>(true);
            _baseCam = canvas.worldCamera;
            
            _spaceText = GetComponentInChildren<TextMeshProUGUI>(true);
            _group = GetComponent<CanvasGroup>();
            
            _brightness = _effect.material.GetFloat(Brightness);
            _gaussianBlur = _effect.material.GetFloat(GaussianBlurFade);
            _alphaDissolve = _effect.material.GetFloat(FullAlphaDissolveFade);

            G.Get<EventBus>().SceneReady += RemoveEffect;
        }

        public async UniTask ApplyEffect()
        {
            canvas.worldCamera = _screenCam;
            _screenCam.gameObject.SetActive(true);

            _group.alpha = 1f;
            _group.blocksRaycasts = true;
            
            _spaceText.gameObject.SetActive(true);
            
            G.Get<EventBus>().EffectApply?.Invoke();
            G.Get<UICursor>().Hide();
            
            G.Get<SoundController>().AnomalyHandle.DerBote1();

            _ = Tween.Custom(
                startValue: 1,
                endValue: _brightness,
                duration: 0.5f,
                onValueChange: value => _effect.material.SetFloat(Brightness, value));

            _ = Tween.Custom(
                startValue: 0,
                endValue: _gaussianBlur,
                duration: 0.5f,
                onValueChange: value => _effect.material.SetFloat(GaussianBlurFade, value));
            
            await Tween.Custom(
                startValue: 1,
                endValue: _alphaDissolve,
                duration: 0.5f,
                onValueChange: value => _effect.material.SetFloat(FullAlphaDissolveFade, value));

            var stepTime = 0.4f;
            var step = 0;
            
            var target = Random.Range(3, 6);
            
            var spaceText = _spaceText.text;
            _spaceText.text = $"[{spaceText}]";
            
            G.Get<InputController>().Player.Player.Space.performed += AddStep;
            
            while (step < target)
            {
                if (stepTime < 0)
                {
                    var brightness = Mathf.MoveTowards(_effect.material.GetFloat(Brightness), _brightness, (1 - _brightness) / target);
                    var gaussianBlur = Mathf.MoveTowards(_effect.material.GetFloat(GaussianBlurFade), _gaussianBlur, _gaussianBlur / target);
                    var alphaDissolve = Mathf.MoveTowards(_effect.material.GetFloat(FullAlphaDissolveFade), _alphaDissolve, (1 - _alphaDissolve) / target);
                    
                    _effect.material.SetFloat(Brightness, brightness);
                    _effect.material.SetFloat(FullAlphaDissolveFade, alphaDissolve);
                    _effect.material.SetFloat(GaussianBlurFade, gaussianBlur);
                    
                    stepTime = 0.4f;
                    step--;
                }

                _ = Tween.Alpha(_spaceText, endValue: 1 - step / (float)target, duration: 0.1f);
                if (step > 0) stepTime -= Time.deltaTime;
                
                await UniTask.Yield();
            }

            await Tween.Alpha(_spaceText, endValue: 0f, duration: 0.3f, ease: Ease.InQuad);
            RemoveEffect();
            
            G.Get<InputController>().Player.Player.Space.performed -= AddStep;
            return;

            void AddStep(InputAction.CallbackContext obj)
            {
                AddStepAsync().Forget();
            }

            async UniTaskVoid AddStepAsync()
            {
                var brightness = Mathf.MoveTowards(_effect.material.GetFloat(Brightness), 1, (1 - _brightness) / target);
                var gaussianBlur = Mathf.MoveTowards(_effect.material.GetFloat(GaussianBlurFade), 0, _gaussianBlur / target);
                var alphaDissolve = Mathf.MoveTowards(_effect.material.GetFloat(FullAlphaDissolveFade), 1, (1 - _alphaDissolve) / target);
                    
                _effect.material.SetFloat(Brightness, brightness);
                _effect.material.SetFloat(FullAlphaDissolveFade, alphaDissolve);
                _effect.material.SetFloat(GaussianBlurFade, gaussianBlur);

                await Tween.Scale(_spaceText.transform, endValue: 0.85f, duration: 0.08f, ease: Ease.OutQuad);
                _spaceText.text = spaceText;
                await Tween.Scale(_spaceText.transform, endValue: 1f, duration: 0.12f, ease: Ease.OutQuad);
                _spaceText.text = $"[{spaceText}]";
                
                step++;
            }
        }
        
        private void RemoveEffect()
        {
            canvas.worldCamera = _baseCam;
            _screenCam.gameObject.SetActive(false);
            
            Tween.Alpha(_group, endValue: 0f, duration: 0.3f, ease: Ease.InQuad).OnComplete(() => _spaceText.gameObject.SetActive(false));
            _group.blocksRaycasts = false;
            
            G.Get<EventBus>().EffectRemove?.Invoke();
            G.Get<UICursor>().Show();
            
            G.Get<SoundController>().AnomalyHandle.Clear();
        }

        private void OnDisable()
        {
            _effect.material.SetFloat(Brightness, _brightness);
            _effect.material.SetFloat(GaussianBlurFade, _gaussianBlur);
            _effect.material.SetFloat(FullAlphaDissolveFade, _alphaDissolve);
        }

        private void OnDestroy()
        {
            G.Get<EventBus>().SceneReady -= RemoveEffect;
        }
        
        private static readonly int Brightness = Shader.PropertyToID("_Brightness");
        private static readonly int GaussianBlurFade = Shader.PropertyToID("_GaussianBlurFade");
        private static readonly int FullAlphaDissolveFade = Shader.PropertyToID("_FullAlphaDissolveFade");
    }
}