using System;
using System.Collections.Generic;
using Tharga.InfluxCapacitor.Collector.Event;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ICounterBusiness
    {
        /// <summary>
        /// Gets or sets additional providers to use.
        /// </summary>
        IEnumerable<IPerformanceCounterProvider> AdditionalProviders { get; set; }

        IEnumerable<IPerformanceCounterGroup> GetPerformanceCounterGroups(IConfig config);

        IEnumerable<string> GetCategoryNames();

        IEnumerable<string> GetCounterNames(string categoryName, string machineName);

        IEnumerable<string> GetInstances(string category, string counterName, string machineName);

        event EventHandler<GetPerformanceCounterEventArgs> GetPerformanceCounterEvent;
    }
}