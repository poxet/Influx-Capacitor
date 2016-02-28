using System;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Influx_Capacitor.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class NullDatabaseConfig : IDatabaseConfig
    {
        public bool IsEnabled { get; private set; }
        public string Url { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Name { get; private set; }
        public TimeSpan? RequestTimeout { get { return null; } }

        public NullDatabaseConfig(bool enabled)
        {
            IsEnabled = enabled;
        }

        public IDataSender GetDataSender(IInfluxDbAgentLoader influxDbAgentLoader, int maxQueueSize)
        {
            return new NullDataSender(maxQueueSize);
        }
    }
}