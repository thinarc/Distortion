using System;
using System.Collections.Generic;
using _Project.Develop.Switch.Scenes;
using Cysharp.Threading.Tasks;

namespace _Project.Develop.Switch
{
    public class SwitchModel
    {
        private readonly Dictionary<string, BaseLocation> _locations = new();
        private BaseLocation _currentLocation;

        private readonly SwitchView _view;
        
        public BaseLocation CurrentLocation => _currentLocation;

        public SwitchModel(SwitchView view)
        {
            _view = view;
        }

        public async UniTask Initialize(string startLocation, BaseLocation[] locations)
        {
            foreach (var location in locations)
            {
                _locations.Add(location.Config.key, location);
                location.Initialize();
            }
            
            await GoToLocation(startLocation);
        }
        
        public async UniTask GoToLocation(string key)
        {
            if (!_locations.TryGetValue(key, out var location)) return;
            await _view.ChangeLocation(current: _currentLocation, target: location);
            _currentLocation = location;
        }
    }
}