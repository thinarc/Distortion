using System;
using System.Threading;
using _Project.Develop.Editing.Tools;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace _Project.Develop.Editing.States
{
    public class EditingState : IEditState, IDisposable
    {
        private readonly EditView _editView;
        private readonly SprayTool _sprayTool;

        private GameObject[] _dustPrefabs;
        private GameObject[] _dirtPrefabs;
        
        private RectTransform _dustContainer;
        private RectTransform _dirtContainer;
        private RectTransform _shineContainer;
        
        private CanvasGroup _spotsGroup;
        
        private const int SpawnDust = 10;
        private static readonly Vector2 ScaleDustRange = new (0.8f, 1.3f);

        private const int SpawnDirt = 5;
        private static readonly Vector2 ScaleDirtRange = new (0.7f, 1.2f);

        private const int MinTargetShine = 10;
        private const int MaxTargetShine = 15;
        
        private int _targetShine;

        private const float ShowPhotoTime = 0.2f;

        private readonly CancellationTokenSource _disposeCts = new();
        
        public EditingState(EditView editView, SprayTool sprayTool)
        {
            _editView = editView;
            _sprayTool = sprayTool;
        }

        public void Initialize()
        {
            _editView.GetPrefabs(out _dustPrefabs, out _dirtPrefabs);
            _editView.GetSpots(out _dustContainer, out _dirtContainer, out _shineContainer);
            
            _sprayTool.Initialize(_shineContainer);
            
            _spotsGroup = _editView.SpotsGroup;
        }
        
        public async UniTask Enter()
        {
            await UniTask.Delay(500);
            
            _editView.PickPhoto(out var photo);
            
            SpawnSpots(photo.Image.rectTransform, out _targetShine);
            photo.gameObject.SetActive(true);

            await UniTask.WaitUntil(() => photo.gameObject.activeInHierarchy, cancellationToken: _disposeCts.Token);
            
            _ = Tween.Alpha(photo.Image, endValue: 1f, duration: ShowPhotoTime);
            _ = Tween.Alpha(_spotsGroup, endValue: 1f, duration: ShowPhotoTime);
            
            await UniTask.WaitUntil(IsSpotsCompleted, cancellationToken: _disposeCts.Token);
            
            G.Get<EditController>().StateController.ChangeState(G.Get<EditController>().RotateState.WithPhoto(photo));
        }

        public async UniTask Exit()
        {
            await UniTask.CompletedTask;
        }
        
        private void SpawnSpots(RectTransform photoRect, out int targetShine)
        {
            ((RectTransform)_spotsGroup.transform.parent).sizeDelta = photoRect.sizeDelta;
            
            _spotsGroup.transform.position = photoRect.position;
            _spotsGroup.transform.rotation = Quaternion.identity;
            
            targetShine = Random.Range(MinTargetShine, MaxTargetShine + 1);
            _sprayTool.MaxShine = targetShine;
            
            Spawn(_dustPrefabs, _dustContainer, SpawnDust, ScaleDustRange);
            Spawn(_dirtPrefabs, _dirtContainer, SpawnDirt, ScaleDirtRange);
            Clear(_shineContainer);
            return;

            void Spawn(GameObject[] prefabs, RectTransform transform, int amount, Vector2 scaleRange)
            {
                for (var i = 0; i < amount; i++)
                {
                    var dust = prefabs[Random.Range(0, prefabs.Length)];

                    var particle = Object.Instantiate(dust, transform);
                    var rt = particle.GetComponent<RectTransform>();

                    var randomX = Random.Range(transform.rect.xMin, transform.rect.xMax);
                    var randomY = Random.Range(transform.rect.yMin, transform.rect.yMax);

                    rt.anchoredPosition = new Vector2(randomX, randomY);

                    var randomScale = Random.Range(scaleRange.x, scaleRange.y);
                    rt.localScale = Vector3.one * randomScale;
                
                    rt.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                }
            }
            
            void Clear(RectTransform container)
            {
                for (var i = 0; i < container.childCount; i++)
                {
                    Object.Destroy(container.GetChild(i).gameObject);
                }
            }
        }

        private bool IsSpotsCompleted()
        {
            return _dustContainer.childCount == 0 && _dirtContainer.childCount == 0 && _shineContainer.childCount >= _targetShine;
        }

        public void Dispose()
        {
            _disposeCts.Cancel();
            _disposeCts.Dispose();
        }
    }
}