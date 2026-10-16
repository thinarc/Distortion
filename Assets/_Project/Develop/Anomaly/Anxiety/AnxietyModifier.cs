namespace _Project.Develop.Anomaly.Anxiety
{
    public class AnxietyModifier
    {
        private readonly float _modifier;
        
        public float Modify(float value) => value * _modifier;
        
        public AnxietyModifier(float value)
        {
            G.Get<AnxietyController>().Apply(this);
            _modifier = value;
        }

        public void Dispose()
        {
            G.Get<AnxietyController>().Remove(this);
        }
    }
}