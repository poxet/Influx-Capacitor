using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Console
{
    public interface ICompositeRoot
    {
        IInfluxDbAgentLoader InfluxDbAgentLoader { get; }
        IConfigBusiness ConfigBusiness { get; }
        ICounterBusiness CounterBusiness { get; }
        IMetaDataBusiness MetaDataBusiness { get; }
        ISendBusiness SendBusiness { get; }
        ITagLoader TagLoader { get; }
        IPublisherBusiness PublisherBusiness { get; }
        ISocketClient SocketClient { get; }
    }
}