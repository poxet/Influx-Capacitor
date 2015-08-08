using System.Collections.Generic;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ICounterBusiness
    {
        List<IPerformanceCounterGroup> GetPerformanceCounterGroups(IConfig config);
        IEnumerable<string> GetCategoryNames();
        IEnumerable<string> GetCounterNames(string category);
        IEnumerable<string> GetInstances(string category, string counterName);
    }
}