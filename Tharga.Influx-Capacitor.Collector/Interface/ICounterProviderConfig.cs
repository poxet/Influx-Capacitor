using System.Reflection;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ICounterProviderConfig
    {
        string Name { get; }

        string Type { get; }

        IPerformanceCounterProvider Load(Assembly defaultAssembly, string defaultNamespace);

        bool HasKey(string key);

        string GetStringValue(string key, string defaultValue = null);

        int GetInt32Value(string key, int defaultValue = 0);

        bool GetBoolValue(string key, bool defaultValue = false);
    }
}