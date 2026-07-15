using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Develop.Editing.Photo
{
    public class EditingChecklist : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            G.Get<EventBus>().ChecklistDown?.Invoke();
        }
    }
}