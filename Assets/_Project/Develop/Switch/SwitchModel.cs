using System.Collections.Generic;

namespace _Project.Develop.Switch
{
    public class SwitchModel
    {
        private Dictionary<string, BaseLocation> _locations = new();
        private string _currentKey;

        private SwitchView _view;

        public SwitchModel(SwitchView view)
        {
            _view = view;
        }

        public void Initialize(string startLocation, BaseLocation[] locations)
        {
            foreach (var loc in locations)
            {
                var data = loc.GetData();
                _locations.Add(data.config.key, data.location);
                
                data.location.Initialize();
            }
            
            GoToLocation(startLocation);
        }
        
        public void GoToLocation(string key)
        {
            if (!_locations.TryGetValue(key, out var loc)) return;
            _currentKey = key;
            _view.ChangeLocation(loc);
        }
    }
}