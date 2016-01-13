using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class KafkaDatabaseConfig : IDatabaseConfig
    {
        public bool IsEnabled { get; }
        public string Url { get; }
        public string Username { get; }
        public string Password { get; }
        public string Name { get; }

        public KafkaDatabaseConfig(bool enabled)
        {
            IsEnabled = enabled;
        }

        public IDataSender GetDataSender(IInfluxDbAgentLoader influxDbAgentLoader, int maxQueueSize)
        {
            return new KafkaDataSender();
        }
    }
}