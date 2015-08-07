using System.Collections.Generic;

namespace InfluxDB.Net.Collector.Interface
{
    public interface ICounterBusiness
    {
        List<IPerformanceCounterGroup> GetPerformanceCounterGroups(IConfig config);
    }
}