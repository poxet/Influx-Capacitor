using System.Collections.Generic;

namespace InfluxDB.Net.Collector.Interface
{
    public interface IConfig
    {
        List<ICounterGroup> Groups { get; }
        IDatabaseConfig Database { get; }
    }
}