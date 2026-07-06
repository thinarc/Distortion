using UnityEngine;

namespace _Project.Develop.Configs
{
    [CreateAssetMenu(fileName = "LocationConfig", menuName = "ScriptableObjects/LocationConfig", order = 1)]
    public class LocationConfig : ScriptableObject
    {
        public DirectionsData directions;
        [Space(5)] public string key;
    }
}