using System;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class KafkaDatabaseConfig : IDatabaseConfig
    {
        public KafkaDatabaseConfig(bool enabled, string url)
        {
            IsEnabled = enabled;
            Url = url;
        }

        public bool IsEnabled { get; }
        public string Url { get; }
        public string Username { get { throw new NotSupportedException(); } }
        public string Password { get { throw new NotSupportedException(); } }
        public string Name { get { return "N/A"; } }

        public IDataSender GetDataSender(IInfluxDbAgentLoader influxDbAgentLoader, int maxQueueSize)
        {
            return new KafkaDataSender(this, maxQueueSize);
        }
    }
}