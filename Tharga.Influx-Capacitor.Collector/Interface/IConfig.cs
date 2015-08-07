using System.Collections.Generic;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface IConfig
    {
        List<ICounterGroup> Groups { get; }
        IDatabaseConfig Database { get; }
    }
}