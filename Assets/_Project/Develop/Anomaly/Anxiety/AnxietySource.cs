using System;

namespace _Project.Develop.Anomaly.Anxiety
{
    public class AnxietySource : IDisposable
    {
        public float EvaluateAnxiety { get; }

        public AnxietySource(float value)
        {
            G.Get<AnxietyController>().Apply(this);
            EvaluateAnxiety = value;
        }

        public void Dispose()
        {
            G.Get<AnxietyController>().Remove(this);
        }
    }
}