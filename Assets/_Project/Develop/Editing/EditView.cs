using System.Collections.Generic;
using _Project.Develop.Anomaly;
using _Project.Develop.Editing.Photo;
using _Project.Develop.Switch.Scenes;
using UnityEngine;
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

        [SerializeField, Space(5)] private RectTransform strikesContainer;
        
        private CanvasGroup _buttonsGroup;
        
        public CanvasGroup SpotsGroup => spotsGroup;
        
        public CanvasGroup ButtonsGroup => _buttonsGroup;
        
        private readonly List<EditingPhoto> _correctPhotos = new();
        private readonly List<AnomalyObject> _anoPhotos = new();
        
        public List<EditingPhoto> CorrectPhotos => _correctPhotos;

        private BaseLocation _discardLocation;

        public void Init()
        {
            _correctPhotos.AddRange(correctRect.GetComponentsInChildren<EditingPhoto>(true));
            _buttonsGroup = buttons[0].GetComponentInParent<CanvasGroup>(true);
        }

        public void GetSpots(out RectTransform dust, out RectTransform dirt, out RectTransform shine)
        {
            var containers = spotsGroup.GetComponentsInChildren<RectTransform>();
            
            dust = containers[1];
            dirt = containers[2];
            shine = containers[3];
        }

        public void GetPrefabs(out GameObject[] dust, out GameObject[] dirt)
        {
            dust = dustPrefabs;
            dirt = dirtPrefabs;
        }

        public void GetStrikes(out Image[] strikes, out EditingChecklist checklist)
        {
            strikes = strikesContainer.GetComponentsInChildren<Image>(true);
            checklist = strikesContainer.GetComponentInParent<EditingChecklist>(true);
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
                photo = _correctPhotos[Random.Range(0, _correctPhotos.Count)];
                _correctPhotos.Remove(photo);
            }
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