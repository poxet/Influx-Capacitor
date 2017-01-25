using System.Collections.Generic;

namespace Tharga.InfluxCapacitor.Interface
{
    public interface IMeasurement
    {
        Dictionary<string, object> Fields { get; }
        Dictionary<string, object> Tags { get; }

        void AddTag(string key, object value);
        void AddField(string key, object value);
    }
}