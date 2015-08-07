using Tharga.InfluxCapacitor.Collector;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Console
{
    public interface ICompositeRoot
    {
        IInfluxDbAgentLoader InfluxDbAgentLoader { get; }
        IConfigBusiness ConfigBusiness { get; }
        Processor Processor { get; }
        ICounterBusiness CounterBusiness { get; }
    }
}