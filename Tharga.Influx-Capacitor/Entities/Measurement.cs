using System;
using System.Collections.Generic;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Entities
{
    public class Measurement : IMeasurement
    {
        private readonly StopwatchHighPrecision _sw;
        private long _pauseTime;
        public Dictionary<string, object> Fields { get; }
        public Dictionary<string, object> Tags { get; }
        public Dictionary<string, TimeSpan> Checkpoints { get; }

        public Measurement()
        {
            _sw = new StopwatchHighPrecision();
            Fields = new Dictionary<string, object>();
            Tags = new Dictionary<string, object>();
            Checkpoints = new Dictionary<string, TimeSpan>();
        }

        internal StopwatchHighPrecision Stopwatch => _sw;

        public void AddTag(string key, object value)
        {
            if (Tags.ContainsKey(key))
                Tags.Remove(key);

            if (string.IsNullOrEmpty(key)) return;
            if (string.IsNullOrEmpty(value?.ToString())) return;

            Tags.Add(key, value);
        }

        public void AddField(string key, object value)
        {
            if (Fields.ContainsKey(key))
                Fields.Remove(key);

            if (string.IsNullOrEmpty(key)) return;
            if (string.IsNullOrEmpty(value?.ToString())) return;

            Fields.Add(key, value);
        }

        public void AddCheckpoint(string name)
        {
            if (Checkpoints.ContainsKey(name))
                Checkpoints.Remove(name);

            if (string.IsNullOrEmpty(name)) return;

            Checkpoints.Add(name, new TimeSpan(_sw.ElapsedTotal - _pauseTime));
        }

        public void Pause()
        {
            _sw.Pause();
        }

        public void Resume()
        {
            _sw.Resume();
        }

        public TimeSpan GetElapsed()
        {
            return new TimeSpan(_sw.ElapsedTotal - _pauseTime);
        }
    }
}