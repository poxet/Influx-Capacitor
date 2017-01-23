using System.Collections.Generic;

namespace Tharga.InfluxCapacitor.Entities
{
    public class Measurement
    {
        public Dictionary<string, object> Fields { get; }
        public Dictionary<string, object> Tags { get; }

        internal Measurement()
        {
            Fields = new Dictionary<string, object>();
            Tags = new Dictionary<string, object>();
        }

        public void AddTag(string key, object value)
        {
            if (Tags.ContainsKey(key))
                Tags.Remove(key);
            Tags.Add(key, value);
        }

        public void AddField(string key, object value)
        {
            if (Fields.ContainsKey(key))
                Fields.Remove(key);
            Fields.Add(key, value);
        }
    }
}