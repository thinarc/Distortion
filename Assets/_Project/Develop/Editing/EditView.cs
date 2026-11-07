using System.Collections.Generic;
using _Project.Develop.Anomaly;
using _Project.Develop.Editing.Photo;
using _Project.Develop.Switch.Scenes;
using UnityEngine;
using PrimeTween;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Project.Develop.Editing
{
    public class EditView : MonoBehaviour
    {
        [SerializeField] private RectTransform correctRect;

        [SerializeField, Space(5)] private CanvasGroup spotsGroup;

        [SerializeField, Space(5)] private GameObject[] dustPrefabs;
        [SerializeField] private GameObject[] dirtPrefabs;

        [SerializeField, Space(5)] private Button[] buttons;

        [SerializeField, Space(5)] private EditingChecklist checklist;
        
        private CanvasGroup _buttonsGroup;
        
        public EditingChecklist Checklist => checklist;
        
        public CanvasGroup SpotsGroup => spotsGroup;
        
        public CanvasGroup ButtonsGroup => _buttonsGroup;
        
        private readonly List<EditingPhoto> _correctPhotos = new();
        private readonly List<AnomalyObject> _anoPhotos = new();
        
        public List<EditingPhoto> CorrectPhotos => _correctPhotos;

        private BaseLocation _discardLocation;
        private EditingPhoto[] _allPhotos;
        private EditingPhoto _currentPhoto;
        private RectTransform _dust;
        private RectTransform _dirt;
        private RectTransform _shine;

        public void CompletePhoto() => _currentPhoto = null;

        public void StopEditing()
        {
            foreach (var t in GetComponentsInChildren<Transform>(true)) Tween.StopAll(t);
            foreach (var image in GetComponentsInChildren<Image>(true)) Tween.StopAll(image);
            Tween.StopAll(spotsGroup);
            Tween.StopAll(_buttonsGroup);
            _buttonsGroup.alpha = 0f;
            _buttonsGroup.interactable = false;
            _buttonsGroup.blocksRaycasts = false;
            checklist.StopEditing();
        }

        public void ResetEditing(bool fullReset)
        {
            if (fullReset)
            {
                _correctPhotos.Clear();
                _correctPhotos.AddRange(_allPhotos);
            }
            else if (_currentPhoto != null && _currentPhoto.Ano == null && !_correctPhotos.Contains(_currentPhoto))
                _correctPhotos.Add(_currentPhoto);

            foreach (var photo in GetComponentsInChildren<EditingPhoto>(true)) photo.ResetPhoto();
            _currentPhoto = null;
            _anoPhotos.Clear();
            spotsGroup.alpha = 0f;
            GetSpots(out var dust, out var dirt, out var shine);
            foreach (var container in new[] { dust, dirt, shine })
                foreach (Transform child in container) Destroy(child.gameObject);
            checklist.ResetChecklist(fullReset);
        }

        public void Init()
        {
            _allPhotos = correctRect.GetComponentsInChildren<EditingPhoto>(true);
            _correctPhotos.AddRange(_allPhotos);
            foreach (var photo in GetComponentsInChildren<EditingPhoto>(true)) photo.CaptureInitialState();
            _buttonsGroup = buttons[0].GetComponentInParent<CanvasGroup>(true);
        }

        public void GetSpots(out RectTransform dust, out RectTransform dirt, out RectTransform shine)
        {
            if (_dust == null)
            {
                var containers = spotsGroup.GetComponentsInChildren<RectTransform>(true);
                _dust = containers[1];
                _dirt = containers[2];
                _shine = containers[3];
            }
            dust = _dust;
            dirt = _dirt;
            shine = _shine;
        }

        public void GetPrefabs(out GameObject[] dust, out GameObject[] dirt)
        {
            dust = dustPrefabs;
            dirt = dirtPrefabs;
        }

        public void BindButtons(UnityAction acceptMethod, UnityAction rejectMethod)
        {
            buttons[0].onClick.AddListener(acceptMethod);
            buttons[1].onClick.AddListener(rejectMethod);
        }

        public void PickPhoto(out EditingPhoto photo)
        {
            if (_anoPhotos.Count > 0)
            {
                var ano = _anoPhotos[Random.Range(0, _anoPhotos.Count)];
                _discardLocation.RemoveAnomaly(ano);
                photo = ano.target.GetComponent<EditingPhoto>();
                photo.Ano = ano;
            }
            else
            {
                if (_correctPhotos.Count == 0) { photo = null; return; }
                photo = _correctPhotos[Random.Range(0, _correctPhotos.Count)];
                _correctPhotos.Remove(photo);
            }
            _currentPhoto = photo;
        }

        public void AddAnomaly(AnomalyObject ano, BaseLocation discardLocation)
        {
            _anoPhotos.Add(ano);
            _discardLocation ??= discardLocation;
        }

        public void DiscardAnomaly(AnomalyObject ano) => _anoPhotos.Remove(ano);

        public void ReturnPhoto(EditingPhoto photo)
        {
            photo.Image.sprite = photo.FrontPhoto;
            _correctPhotos.Add(photo);
        }
    }
}