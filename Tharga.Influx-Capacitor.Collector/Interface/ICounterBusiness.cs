using System.Collections.Generic;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ICounterBusiness
    {
        List<IPerformanceCounterGroup> GetPerformanceCounterGroups(IConfig config);
    }
}