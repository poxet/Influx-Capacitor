using System.Collections.Generic;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface IConfig
    {
        List<ICounterGroup> Groups { get; }
        List<ICounterPublisher> Publishers { get; }
        List<IDatabaseConfig> Databases { get; }
        IApplicationConfig Application { get; }
        List<ICounterProviderConfig> Providers { get; }
        List<ITag> Tags { get; }
    }
}