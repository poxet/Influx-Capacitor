using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Console
{
    public interface ICompositeRoot
    {
        IInfluxDbAgentLoader InfluxDbAgentLoader { get; }
        IConfigBusiness ConfigBusiness { get; }
        ICounterBusiness CounterBusiness { get; }
        ISendBusiness SendBusiness { get; }
    }
}