using System.Linq;
using _Project.Develop.Switch.Scenes;
using Sirenix.Utilities;
using UnityEngine;

namespace _Project.Develop.Anomaly
{
    public class AnomalyView : MonoBehaviour
    {
        [SerializeField] private BaseLocation[] roomLocations;
        [SerializeField, Space(5)] private BaseLocation streetLocation;

        private void Start()
        {
            G.Get<EventBus>().SceneReset += RemoveAll;
        }
        
        public void SpawnInRoom()
        {
            foreach (var loc in roomLocations.OrderBy(_ => Random.value))
            {
                if (loc.ApplyAnomaly())
                    break;
            }
        }
        
        public void SpawnOnStreet()
        {
            streetLocation.ApplyAnomaly();
        }
        
        public void RemoveFromRoom()
        {
            foreach (var loc in roomLocations.OrderBy(_ => Random.value))
            {
                if (loc.RemoveAnomaly())
                    break;
            }
        }
        
        public void RemoveFromStreet()
        {
            streetLocation.RemoveAnomaly();
        }

        public void RemoveAllFromRoom()
        {
            roomLocations.ForEach(l => l.RemoveAllAnomaly());
        }

        public void RemoveAllFromStreet()
        {
            streetLocation.RemoveAllAnomaly();
        }

        public void RemoveAll()
        {
            RemoveAllFromStreet();
            RemoveAllFromRoom();
        }
        
        private void OnDestroy()
        {
            G.Get<EventBus>().SceneReset -= RemoveAll;
        }
    }
}