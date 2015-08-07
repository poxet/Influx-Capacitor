namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface IInfluxDbAgentLoader
    {
        IInfluxDbAgent GetAgent(IDatabaseConfig database);
    }
}