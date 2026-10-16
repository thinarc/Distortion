using System.Globalization;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace _Project.Develop.UI.Menus
{
    public class LanguageSelector : MonoBehaviour
    {
        private CanvasGroup _group;

        private bool _selected;
        
        public void Init()
        {
            _group = GetComponent<CanvasGroup>();
        }

        public async UniTask Show(float duration, float startDelay)
        {
            if (transform.childCount == 1)
            {
                await LocalizationSettings.InitializationOperation.Task;
                
                var locales = LocalizationSettings.AvailableLocales.Locales;
                var selectorPrefab = transform.GetChild(0).gameObject;

                var initPrefab = false;

                foreach (var locale in locales)
                {
                    GameObject selector;
                    
                    if (!initPrefab)
                    {
                        selector = selectorPrefab;
                        initPrefab = true;
                    }
                    else selector = Instantiate(selectorPrefab, transform);
                    
                    var culture = locale.Identifier.CultureInfo;
                    var labelText = CultureInfo.GetCultureInfo(culture.TwoLetterISOLanguageName).NativeName;

                    labelText = char.ToUpper(labelText[0]) + labelText[1..];
                    
                    selector.GetComponent<Button>().onClick.AddListener(() => Select(locale));
                    selector.GetComponentInChildren<TextMeshProUGUI>().text = labelText;
                }
            }
            
            await Tween.Alpha(_group, endValue: 1f, duration: duration, startDelay: startDelay);
            _group.blocksRaycasts = true;
            _group.interactable = true;
            
            await UniTask.WaitUntil(() => _selected);
            _group.blocksRaycasts = false;
            _group.interactable = false;
        }

        private void Select(Locale locale)
        {
            LocalizationSettings.SelectedLocale = locale;
            _selected = true;
        }
        
        public async UniTask Hide(float duration)
        {
            await Tween.Alpha(_group, endValue: 0f, duration: duration);
        }
    }
}