using System;
using System.Collections.Generic;
using _Project.Develop.Editing.Photo;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Project.Develop.Editing
{
    public class EditView : MonoBehaviour
    {
        [SerializeField] private RectTransform correctRect;
        [SerializeField] private RectTransform oddRect;

        [SerializeField, Space(5)] private CanvasGroup spotsGroup;

        [SerializeField, Space(5)] private GameObject[] dustPrefabs;
        [SerializeField] private GameObject[] dirtPrefabs;

        [SerializeField, Space(5)] private Button[] buttons;

        [SerializeField, Space(5)] private RectTransform strikesContainer;
        
        private CanvasGroup _buttonsGroup;
        
        public CanvasGroup SpotsGroup => spotsGroup;
        
        public CanvasGroup ButtonsGroup => _buttonsGroup;
        
        private readonly List<EditingPhoto> _correctPhotos = new();
        private readonly List<EditingPhoto> _oddPhotos = new();

        public void Initialize()
        {
            _correctPhotos.AddRange(correctRect.GetComponentsInChildren<EditingPhoto>(true));
            _oddPhotos.AddRange(oddRect.GetComponentsInChildren<EditingPhoto>(true));
            
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

        public void GetStrikes(out Image[] strikes)
        {
            strikes = strikesContainer.GetComponentsInChildren<Image>(true);
        }

        public void BindButtons(UnityAction acceptMethod, UnityAction rejectMethod)
        {
            buttons[0].onClick.AddListener(acceptMethod);
            buttons[1].onClick.AddListener(rejectMethod);
        }

        public void PickPhoto(out EditingPhoto photo, bool pickOdd = false)
        {
            var collection = pickOdd ? _oddPhotos : _correctPhotos;
            if (collection.Count == 0) throw new IndexOutOfRangeException($"No more photos to pick in {(pickOdd ? "odd" : "correct")} photos list.");
            
            photo = collection[Random.Range(0, collection.Count)];
            collection.Remove(photo);
        }

        public void ReturnPhoto(EditingPhoto photo)
        {
            _correctPhotos.Add(photo);
        }
    }
}