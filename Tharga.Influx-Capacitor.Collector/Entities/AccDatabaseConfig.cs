using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class AccDatabaseConfig : IDatabaseConfig
    {
        public string Url { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Name { get; private set; }

        public IDataSender GetDataSender(IInfluxDbAgentLoader influxDbAgentLoader, int maxQueueSize)
        {
            return new AccDataSender(maxQueueSize);
        }
    }
}