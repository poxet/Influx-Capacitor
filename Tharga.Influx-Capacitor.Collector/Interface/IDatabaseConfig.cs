using System;
using Tharga.Influx_Capacitor.Interface;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface IDatabaseConfig
    {
        bool IsEnabled { get; }
        string Url { get; }
        string Username { get; }
        string Password { get; }
        string Name { get; }
        TimeSpan? RequestTimeout { get; }
        IDataSender GetDataSender(IInfluxDbAgentLoader influxDbAgentLoader, int maxQueueSize);
    }
}