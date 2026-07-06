using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Switch
{
    public class SwitchView : MonoBehaviour
    {
        [SerializeField] private Button[] buttons;

        private CanvasGroup _group;

        private void Start()
        {
            _group = GetComponent<CanvasGroup>();
        }
        
        public async void ChangeLocation(BaseLocation target)
        {
            await target.Show();
            _ = Tween.Alpha(_group, endValue: 1f, duration: 0.2f, ease: Ease.InOutSine);
        }
    }
}