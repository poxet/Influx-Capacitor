using System.Collections.Generic;

namespace Tharga.InfluxCapacitor.Interface
{
    public interface IMeasurement
    {
        Dictionary<string, object> Fields { get; }
        Dictionary<string, object> Tags { get; }
        Dictionary<string, double> Checkpoints { get; }

        void AddTag(string key, object value);
        void AddField(string key, object value);

        void AddCheckpoint(string name);
    }
}