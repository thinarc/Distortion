using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace _Project.Develop.UI
{
    public class AdaptiveTextColor : MonoBehaviour
    {
        [SerializeField] private Color lightColor = Color.white;
        [SerializeField] private Color darkColor = Color.black;

        [SerializeField, Space(5), Range(0f, 1f)] private float threshold = 0.5f;

        private const int SamplesX = 8;
        private const int SamplesY = 4;

        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            UpdateContrastAsync().Forget();
        }

        private async UniTaskVoid UpdateContrastAsync()
        {
            await UniTask.WaitForEndOfFrame();

            if (!isActiveAndEnabled)
                return;

            UpdateContrast();
        }

        private void UpdateContrast()
        {
            var rect = _text.rectTransform;

            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);

            var cam = _text.canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : _text.canvas.worldCamera;

            var min = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
            var max = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);

            var width = max.x - min.x;
            var height = max.y - min.y;

            var texture = new Texture2D(SamplesX, SamplesY, TextureFormat.RGB24, false);

            texture.ReadPixels(new Rect(min.x, min.y, width, height), 0, 0);
            texture.Apply();

            var brightness = 0f;

            for (var x = 0; x < SamplesX; x++)
            {
                for (var y = 0; y < SamplesY; y++)
                    brightness += GetLuminance(texture.GetPixel(x, y));
            }

            Destroy(texture);

            brightness /= SamplesX * SamplesY;
            _text.color = brightness > threshold ? darkColor : lightColor;
        }

        private static float GetLuminance(Color color) => color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f;
    }
}