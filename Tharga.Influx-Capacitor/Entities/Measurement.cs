using System.Collections.Generic;
using System.Diagnostics;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Entities
{
    public class Measurement : IMeasurement
    {
        private readonly Stopwatch _sw;
        public Dictionary<string, object> Fields { get; }
        public Dictionary<string, object> Tags { get; }

        public Measurement()
            : this(new Stopwatch())
        {
        }

        private Measurement(Stopwatch sw)
        {
            _sw = sw;
            Fields = new Dictionary<string, object>();
            Tags = new Dictionary<string, object>();
        }

        public Stopwatch Stopwatch => _sw;

        public void AddTag(string key, object value)
        {
            if (Tags.ContainsKey(key))
                Tags.Remove(key);

            if (string.IsNullOrEmpty(value?.ToString())) return;

            Tags.Add(key, value);
        }

        public void AddField(string key, object value)
        {
            if (Fields.ContainsKey(key))
                Fields.Remove(key);

            if (string.IsNullOrEmpty(value?.ToString())) return;

            Fields.Add(key, value);
        }

        public void Elapsed(string positionName)
        {
            AddField($"Elapsed-{positionName}", _sw.Elapsed);
        }
    }
}