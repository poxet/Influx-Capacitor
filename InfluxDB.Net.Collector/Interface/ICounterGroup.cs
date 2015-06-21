using System.Collections.Generic;

namespace InfluxDB.Net.Collector.Interface
{
    public interface ICounterGroup
    {
        string Name { get; }
        int SecondsInterval { get; }
        IEnumerable<ICounter> Counters { get; }
    }
}