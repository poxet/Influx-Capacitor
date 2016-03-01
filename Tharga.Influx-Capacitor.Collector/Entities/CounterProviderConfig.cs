using System;
using System.Collections.Generic;
using System.Reflection;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class CounterProviderConfig : ICounterProviderConfig
    {
        private readonly string _name;
        private readonly string _type;
        private readonly IDictionary<string, string> _config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public CounterProviderConfig(string name, string type, IDictionary<string, string> config)
        {
            _name = name;
            _type = type;
            
            // we copy values in a specific case insensitive dictionary
            if (config != null)
            {
                foreach (var pair in config)
                {
                    _config[pair.Key] = pair.Value;
                }
            }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Type
        {
            get { return _type; }
        }

        public IPerformanceCounterProvider Load(Assembly defaultAssembly, string defaultNamespace)
        {
            var typeName = _type;
            Type type;
            if (typeName.IndexOf(",", StringComparison.Ordinal) == -1)
            {
                // no assembly separator, the type must be present in defaultAssembly
                if (typeName.IndexOf(".", StringComparison.Ordinal) == -1)
                {
                    // no namespace, the type must be present in defaultNamespace
                    typeName = defaultNamespace + "." + _type;
                }
                else
                {
                    typeName = _type;
                }

                type = defaultAssembly.GetType(typeName);
                if (type == null)
                {
                    throw new InvalidOperationException(string.Format("Unable to load type {0} from default assembly {1}", typeName, defaultAssembly));
                }
            }
            else
            {
                type = System.Type.GetType(_type);
                if (type == null)
                {
                    throw new InvalidOperationException(string.Format("Unable to load type {0}", _type));
                }
            }

            var instance = Activator.CreateInstance(type) as IPerformanceCounterProvider;
            if (instance == null)
            {
                throw new InvalidOperationException(string.Format("Unable to cast instance of {0} as IPerformanceCounterProvider", typeName));
            }

            return instance;
        }

        public bool HasKey(string key)
        {
            return _config.ContainsKey(key);
        }

        public string GetStringValue(string key, string defaultValue = null)
        {
            string value;
            return _config.TryGetValue(key, out value) ? value : defaultValue;
        }

        public int GetInt32Value(string key, int defaultValue = 0)
        {
            var textValue = GetStringValue(key);
            if (string.IsNullOrEmpty(textValue))
                return defaultValue;

            int value;
            if (!int.TryParse(textValue, out value))
            {
                throw new ArgumentException(string.Format("The provider configuration value named {0} must be an integer. Current value: {1}", key, textValue));
            }

            return value;
        }

        public bool GetBoolValue(string key, bool defaultValue = false)
        {
            var textValue = GetStringValue(key);
            if (string.IsNullOrEmpty(textValue))
                return defaultValue;

            bool value;
            if (!bool.TryParse(textValue, out value))
            {
                throw new ArgumentException(string.Format("The provider configuration value named {0} must be a boolean with a value of 'true' or 'false'. Current value: {1}", key, textValue));
            }

            return value;
        }
    }
}