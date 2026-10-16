using TMPro;
using UnityEngine;

namespace _Project.Develop.UI.HUD
{
    public class UIStats : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI counterText;
        [SerializeField] private TextMeshProUGUI timerText;
        
        private float _nextUpdateTime;
        private int _lastHour;

        private int _completedPhotos;
        private int _targetPhotos;

        private int _completedNights;

        private bool _training;

        public float Timer { get; set; }
        
        private void Start()
        {
            OnSceneReset();
            
            G.Get<EventBus>().ChecklistDown += UpdateCounter;
            G.Get<EventBus>().SceneReset += OnSceneReset;
        }

        private void OnSceneReset()
        {
            _completedPhotos = -1;
            _targetPhotos = 3;

            Timer = 0f;
            
            UpdateTimer(23, 0);
            UpdateCounter();
            
            // _training = true;
            
            _completedNights = PlayerPrefs.GetInt("completed-nights", -1);
        }

        private void Update()
        {
            Timer += Time.deltaTime;
            
            if (_training)
            {
                var progress = Mathf.Clamp01(Timer / 60f);
                var minutes = Mathf.FloorToInt(progress * 59f);
                
                UpdateTimer(23, minutes);
            }
            else
            {
                var progress = Mathf.Clamp01(Timer / 360f);
                var totalMinutes = progress * 360f;
                var hours = Mathf.FloorToInt(totalMinutes / 60f);
                var minutes = Mathf.FloorToInt(totalMinutes % 60f);
                
                UpdateTimer(hours, minutes);
                if (hours == 6) G.Get<EventBus>().AverageEnd?.Invoke();
            }
        }
        
        public void CompleteTraining()
        {
            _training = false;
            Timer = 0f;
            UpdateTimer(0, 0);
        }
        
        public void SkipUpdateDelay()
        {
            _nextUpdateTime = 0f;
        }
        
        private void UpdateTimer(int hours, int minutes)
        {
            if (hours == _lastHour && Time.time < _nextUpdateTime)
                return;

            _nextUpdateTime = Time.time + Random.Range(2f, 10f);
            _lastHour = hours;
            
            timerText.text = $"{hours}:{minutes:00}";
        }

        private void UpdateCounter()
        {
            counterText.text = ++_completedPhotos + $"/{_targetPhotos}";

            if (_completedPhotos >= _targetPhotos)
            {
                if (++_completedNights >= 3) G.Get<EventBus>().GoodEnd?.Invoke();
                else G.Get<EventBus>().PassNight?.Invoke(_completedNights);
                
                PlayerPrefs.SetInt("completed-nights", _completedNights);
            }
        }

        private void OnDestroy()
        {
            G.Get<EventBus>().ChecklistDown -= UpdateCounter;
            G.Get<EventBus>().SceneReset -= OnSceneReset;
        }
    }
}