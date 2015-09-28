using System.Collections.Generic;
using Tharga.InfluxCapacitor.Collector.Handlers;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ICounterGroup
    {
        string Name { get; }
        int SecondsInterval { get; }
        int RefreshInstanceInterval { get; }
        IEnumerable<ICounter> Counters { get; }
        IEnumerable<ITag> Tags { get; }
        CollectorEngineType CollectorEngineType { get; }
    }
}