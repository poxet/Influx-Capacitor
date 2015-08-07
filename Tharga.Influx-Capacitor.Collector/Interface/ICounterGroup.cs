using System.Collections.Generic;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ICounterGroup
    {
        string Name { get; }
        int SecondsInterval { get; }
        IEnumerable<ICounter> Counters { get; }
    }
}