using System;
using System.Linq;
using _Project.Develop.Input;
using _Project.Develop.Sound;
using _Project.Develop.UI;
using _Project.Develop.UI.Cursor;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Project.Develop.Editing.Photo
{
    public class EditingChecklist : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [SerializeField] private RectTransform strikesContainer;
        [SerializeField] private RectTransform stringsContainer;

        [SerializeField, Space(5)] private Sprite rotatedList;

        private Image _image;
        private Sprite _baseList;
        
        private Image[] _strikes;
        private TextMeshProUGUI[] _strings;

        private readonly float[] _strikesContained = new float[19];
        
        private UIEffects[] _stringsEffects;
        private EventTrigger[] _stringsTriggers;
        
        private UIEffects _effects;
        private CursorToolTrigger _toolTrigger;

        private int _numberToCrossOut = -1;
        private string[] _splitTexts;
        
        private Vector2 _pointerStartPosition;
        private bool _isRotating;
        
        private bool _rotated;
        private int _revision;

        public void StopEditing()
        {
            _revision++;
            _numberToCrossOut = -1;
            _isRotating = false;
            _effects.SetEffect(UIEffectType.PressScale, false);
            _toolTrigger.Disable();
            if (_stringsEffects == null) return;
            foreach (var effect in _stringsEffects) effect.enabled = false;
            foreach (var trigger in _stringsTriggers) trigger.enabled = false;
        }

        public void ResetChecklist(bool fullReset)
        {
            if (fullReset)
            {
                Array.Clear(_strikesContained, 0, _strikesContained.Length);
                _rotated = false;
            }
            _image.sprite = _rotated ? rotatedList : _baseList;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            for (var i = 0; i < 10; i++)
            {
                var index = i + (_rotated ? 10 : 0);
                _strikes[i].fillAmount = index < _strikesContained.Length ? _strikesContained[index] : 0f;
                if (_splitTexts != null) _strings[i].text = index < _splitTexts.Length ? _splitTexts[index] : "";
            }
        }

        private void OnDestroy() => _revision++;

        private void Start()
        {
            _image = GetComponent<Image>();
            _baseList = _image.sprite;
            
            _strikes = strikesContainer.GetComponentsInChildren<Image>();
            
            _effects = GetComponent<UIEffects>();
            _toolTrigger = GetComponent<CursorToolTrigger>();
            
            _effects.SetEffect(UIEffectType.PressScale, false);
            Disable().Forget();
        }

        private async UniTaskVoid SimulateHover()
        {
            var revision = _revision;
            _effects.OnPointerEnter(null);
            await UniTask.Delay(180);
            if (revision != _revision) return;
            _effects.OnPointerExit(null);
        }

        public void CrossOut()
        {
            CrossOutAsync().Forget();
        }
        
        private async UniTaskVoid CrossOutAsync()
        {
            var revision = _revision;
            G.Get<SoundController>().ActionHandle.PlayCross();
            _effects.SetEffect(UIEffectType.PressScale, true);
            
            if (_numberToCrossOut <= 10 && !_rotated)
            {
                _stringsEffects[_numberToCrossOut].enabled = false;
                _stringsTriggers[_numberToCrossOut].enabled = false;
            }
            else if (_numberToCrossOut > 10 && _rotated)
            {
                _stringsEffects[_numberToCrossOut - 10].enabled = false;
                _stringsTriggers[_numberToCrossOut - 10].enabled = false;
            }
            
            _effects.OnPointerDown(null);

            
            Image strike;
            
            if (_numberToCrossOut <= 10 && !_rotated)
                strike = _strikes[_numberToCrossOut];
            else if (_numberToCrossOut > 10 && _rotated)
                strike = _strikes[_numberToCrossOut - 10];
            else throw new IndexOutOfRangeException();
            
            var t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime * 3f;
                
                var noise = Mathf.PerlinNoise(Time.time * 18f, 0f) * 0.08f;
                
                strike.fillAmount = Mathf.Clamp01(t + noise);
                
                await UniTask.Yield();
                if (revision != _revision) return;
            }

            _strikesContained[_numberToCrossOut] = strike.fillAmount;
            _numberToCrossOut = -1;
            
            _effects.OnPointerUp(null);


            await UniTask.Delay(Random.Range(450, 700));
            if (revision != _revision) return;

            _effects.SetEffect(UIEffectType.PressScale, false);
            Disable().Forget();
            
            G.Get<EditController>().CompletePhoto();
            G.Get<EventBus>().ChecklistDown?.Invoke();
            return;


        }

        public void SetText(string text)
        {
            _strings ??= stringsContainer.GetComponentsInChildren<TextMeshProUGUI>();
            _stringsEffects ??= _strings.Select(e => e.GetComponent<UIEffects>()).ToArray();
            _stringsTriggers ??= _strings.Select(t => t.GetComponent<EventTrigger>()).ToArray();
            
            _splitTexts = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            
            for (var i = 0; i < 10; i++)
                _strings[i].text = _splitTexts[i];
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isRotating) return;
            _pointerStartPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isRotating) return;

            var dragDelta = eventData.position - _pointerStartPosition;
            
            if (Mathf.Abs(dragDelta.x) > 50f)
                if ((dragDelta.x < 0 && !_rotated) || (dragDelta.x > 0 && _rotated))
                    RotateChecklistAsync().Forget();
        }

        private async UniTaskVoid RotateChecklistAsync()
        {
            var revision = _revision;
            if (_numberToCrossOut is <= 10 and > -1 && !_rotated)
            {
                _stringsEffects[_numberToCrossOut].enabled = false;
                _stringsTriggers[_numberToCrossOut].enabled = false;
            }
            else if (_numberToCrossOut > 10 && _rotated)
            {
                _stringsEffects[_numberToCrossOut - 10].enabled = false;
                _stringsTriggers[_numberToCrossOut - 10].enabled = false;
            }
            
            _isRotating = true;
            _rotated = !_rotated;
            
            var midEuler = new Vector3(transform.localEulerAngles.x, 90f, transform.localEulerAngles.z);
            await Tween.LocalRotation(transform, midEuler, duration: 0.25f, ease: Ease.InQuad);
            if (revision != _revision) return;

            if (_rotated)
            {
                _image.sprite = rotatedList;
                
                for (var i = 0; i < 9; i++)
                    _strings[i].text = _splitTexts[i + 10];

                _strings[^1].text = "";
                
                for (var i = 0; i < 9; i++)
                    _strikes[i].fillAmount = _strikesContained[i + 10];
                
                _strikes[^1].fillAmount = 0;
            }
            else
            {
                _image.sprite = _baseList;
                
                for (var i = 0; i < 10; i++)
                    _strings[i].text = _splitTexts[i];

                for (var i = 0; i < 10; i++)
                    _strikes[i].fillAmount = _strikesContained[i];
            }
            
            var targetEuler = new Vector3(transform.localEulerAngles.x, 0f, transform.localEulerAngles.z);
            await Tween.LocalRotation(transform, targetEuler, duration: 0.25f, ease: Ease.OutQuad);
            if (revision != _revision) return;
            
            if (_numberToCrossOut is <= 10 and > -1 && !_rotated)
            {
                _stringsEffects[_numberToCrossOut].enabled = true;
                _stringsTriggers[_numberToCrossOut].enabled = true;
            }
            else if (_numberToCrossOut > 10 && _rotated)
            {
                _stringsEffects[_numberToCrossOut - 10].enabled = true;
                _stringsTriggers[_numberToCrossOut - 10].enabled = true;
            }
            
            _isRotating = false;
        }

        private async UniTaskVoid Disable()
        {
            var revision = _revision;
            await UniTask.Delay(700);
            if (revision != _revision) return;
            
            _toolTrigger.Disable();
        }

        public void Enable(int numberToCrossOut)
        {
            _toolTrigger.Enable();
            
            _numberToCrossOut = numberToCrossOut;

            if (_numberToCrossOut <= 10 && !_rotated)
            {
                _stringsEffects[_numberToCrossOut].enabled = true;
                _stringsTriggers[_numberToCrossOut].enabled = true;
            }
            else if (_numberToCrossOut > 10 && _rotated)
            {
                _stringsEffects[_numberToCrossOut - 10].enabled = true;
                _stringsTriggers[_numberToCrossOut - 10].enabled = true;
            }
            
            SimulateHover().Forget();
        }
    }
}
