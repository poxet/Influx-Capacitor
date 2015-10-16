using System.Collections.Generic;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class PublisherBusiness : IPublisherBusiness
    {
        public IEnumerable<ICounterPublisher> GetCounterPublishers(IConfig config)
        {
            return config.Publishers;
        }
    }
}