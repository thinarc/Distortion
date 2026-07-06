using _Project.Develop.Configs;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Develop.Switch
{
    public abstract class BaseLocation : MonoBehaviour
    {
        [SerializeField] private LocationConfig directions;

        [SerializeField, Space(5)] private float transitionTime = 0.2f;
        [FormerlySerializedAs("transition")] [SerializeField] private Ease transitionEase = Ease.Linear;

        private CanvasGroup _group;

        public void Initialize()
        {
            _group = GetComponent<CanvasGroup>();
            _group.alpha = 0;
        }

        public async UniTask Show()
        {
            gameObject.SetActive(true);
            await Tween.Alpha(_group, endValue: 1f, duration: transitionTime, ease: transitionEase);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public LocationData GetData()
        {
            return new LocationData
            {
                config = directions,
                location = this
            };
        }
    }
}