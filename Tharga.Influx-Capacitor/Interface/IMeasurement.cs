using System;
using System.Collections.Generic;

namespace Tharga.InfluxCapacitor.Interface
{
    public interface IMeasurement
    {
        Dictionary<string, object> Fields { get; }
        Dictionary<string, object> Tags { get; }
        Dictionary<string, TimeSpan> Checkpoints { get; }
        void Pause();
        void Resume();

        void AddTag(string key, object value);
        void AddField(string key, object value);
        void AddCheckpoint(string name);

        TimeSpan GetElapsed();
    }
}