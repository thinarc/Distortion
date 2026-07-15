using System;

namespace _Project.Develop.Switch
{
    public class SwitchController
    {
        private readonly SwitchModel _model;
        
        public SwitchController(SwitchModel model)
        {
            _model = model;
        }

        private void MoveTo(string direction)
        {
            if (string.IsNullOrEmpty(direction)) throw new NullReferenceException("Direction is empty");
            _ = _model.GoToLocation(direction);
        }

        public void MoveLeft() => MoveTo(_model.CurrentLocation.Config.directions.left);

        public void MoveRight() => MoveTo(_model.CurrentLocation.Config.directions.right);
        
        public void MoveBottom() => MoveTo(_model.CurrentLocation.Config.directions.bottom);
        
        public void MoveUp() => MoveTo(_model.CurrentLocation.Config.directions.up);
        
        public void MoveThings() => MoveTo(_model.CurrentLocation.Config.directions.things);
    }
}