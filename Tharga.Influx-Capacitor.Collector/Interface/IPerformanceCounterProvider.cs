using System.Collections.Generic;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    /// <summary>
    /// Defines a provider, able to returns a <see cref="IPerformanceCounterGroup"/> based on a <see cref="ICounterGroup"/>, and able to give informations about ambient categories, counter names and instances.
    /// </summary>
    public interface IPerformanceCounterProvider
    {
        string Name { get; }

        IPerformanceCounterGroup GetGroup(ICounterGroup group);

        IEnumerable<string> GetCategoryNames();

        IEnumerable<string> GetCounterNames(string categoryName, string machineName);

        IEnumerable<string> GetInstances(string category, string counterName, string machineName);
    }
}