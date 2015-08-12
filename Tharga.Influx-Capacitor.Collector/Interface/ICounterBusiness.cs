using System;
using System.Collections.Generic;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Event;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ICounterBusiness
    {
        List<IPerformanceCounterGroup> GetPerformanceCounterGroups(IConfig config);
        IEnumerable<string> GetCategoryNames();
        IEnumerable<string> GetCounterNames(string category);
        IEnumerable<string> GetInstances(string category, string counterName);
        event EventHandler<GetPerformanceCounterEventArgs> GetPerformanceCounterEvent;
    }
}