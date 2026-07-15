using _Project.Develop.Editing.Photo;
using _Project.Develop.Input;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Develop.Editing.Tools
{
    public class SprayTool : PickupTool
    {
        [SerializeField, Space(5)] private GameObject[] shinePrefabs;
        
        private RectTransform _shineContainer;
        
        private ParticleSystem _sprayParticles;
        
        private float _spawnInterval;
        
        private bool _isOnPhoto;
        private bool _isOnSpot;
        
        private bool IsOnPhoto { set => _isOnPhoto = value; }
        private bool IsOnSpot { set => _isOnSpot = value; }
        
        private int _maxShine;
        
        public int MaxShine { set => _maxShine = value * 2; }

        private static readonly Vector2 SpawnOffset = new(150f, 150f);
        private const float SpawnInterval = 0.1f;
        
        private static readonly Vector2 ScaleRange = new(0.9f, 1.4f);

        protected override void Start()
        {
            base.Start();
            _sprayParticles = GetComponentInChildren<ParticleSystem>();
        }
        
        public void Initialize(RectTransform shineContainer)
        {
            _shineContainer = shineContainer;
            
            var eventBus = G.Get<EventBus>();

            eventBus.PhotoEnter += OnPhotoEnter;
            eventBus.PhotoExit += OnPhotoExit;
            eventBus.DustEnter += OnSpotEnter;
            eventBus.DirtEnter += OnSpotEnter;
        }
        
        protected override async void PickUp()
        {
            base.PickUp();

            while (Picked)
            {
                await UniTask.Yield();
                
                if (!_isOnPhoto)
                {
                    _sprayParticles.Stop();
                    continue;
                }
                
                _spawnInterval -= Time.deltaTime;
                _sprayParticles.Play();
                
                if (CanSpawn()) SpawnShine();
            }
        }
        
        protected override void Drop(InputAction.CallbackContext obj)
        {
            base.Drop(obj);
            _sprayParticles.Stop();
        }

        private bool CanSpawn()
        {
            return !_isOnSpot && _shineContainer.childCount < _maxShine && _spawnInterval < 0;
        }
        
        private void SpawnShine()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _shineContainer,
                G.Get<InputController>().Player.Player.MousePosition.ReadValue<Vector2>(),
                Canvas.worldCamera,
                out var localPoint
            );
            
            var prefab = shinePrefabs[Random.Range(0, shinePrefabs.Length)];
            var size = Random.Range(ScaleRange.x, ScaleRange.y);
            
            var shine = Instantiate(prefab, _shineContainer);
            var rect = shine.GetComponent<RectTransform>();
            
            rect.anchoredPosition = localPoint + SpawnOffset;
            rect.localScale = Vector3.one * size;
            rect.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            
            var pos = rect.localPosition;
            pos.z = 0;
            rect.localPosition = pos;

            _spawnInterval = SpawnInterval;
        }
        
        private void OnPhotoEnter()
        {
            IsOnPhoto = true;
            IsOnSpot = false;
        }

        private void OnPhotoExit() => IsOnPhoto = false;

        protected override void OnSpotEnter(EditingSpot spot) => IsOnSpot = true;

        private void OnDestroy()
        {
            var eventBus = G.Get<EventBus>();

            eventBus.PhotoEnter -= OnPhotoEnter;
            eventBus.PhotoExit -= OnPhotoExit;
            eventBus.DustEnter -= OnSpotEnter;
            eventBus.DirtEnter -= OnSpotEnter;
        }
    }
}