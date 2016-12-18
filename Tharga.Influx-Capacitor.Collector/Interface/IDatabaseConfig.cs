using System;
using Tharga.InfluxCapacitor.Interface;

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
        ISenderAgent GetSenderAgent();

        //[Obsolete("Use GetSenderAgent instead.")]
        //IDataSender GetDataSender(IInfluxDbAgentLoader influxDbAgentLoader, int maxQueueSize);
    }
}