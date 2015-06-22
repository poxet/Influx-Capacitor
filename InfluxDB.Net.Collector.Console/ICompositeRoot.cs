using InfluxDB.Net.Collector.Interface;

namespace InfluxDB.Net.Collector.Console
{
    public interface ICompositeRoot
    {
        IInfluxDbAgentLoader InfluxDbAgentLoader { get; }
        IConfigBusiness ConfigBusiness { get; }
        Processor Processor { get; }
    }
}