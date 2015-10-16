using System.Collections.Generic;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface IPublisherBusiness
    {
        IEnumerable<ICounterPublisher> GetCounterPublishers(IConfig config);
    }
}