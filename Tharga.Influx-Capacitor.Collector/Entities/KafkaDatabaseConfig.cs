using System;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Influx_Capacitor.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class KafkaDatabaseConfig : IDatabaseConfig
    {
        public KafkaDatabaseConfig(bool enabled, string url)
        {
            IsEnabled = enabled;
            Url = url;
        }

        public bool IsEnabled { get; private set; }
        public string Url { get; private set; }
        public string Username { get { throw new NotSupportedException(); } }
        public string Password { get { throw new NotSupportedException(); } }
        public string Name { get { return "N/A"; } }
        public TimeSpan? RequestTimeout { get { return null; } }

        public IDataSender GetDataSender(IInfluxDbAgentLoader influxDbAgentLoader, int maxQueueSize)
        {
            return new KafkaDataSender(this, maxQueueSize);
        }
    }
}