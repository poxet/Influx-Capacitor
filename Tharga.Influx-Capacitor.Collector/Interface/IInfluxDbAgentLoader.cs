namespace InfluxDB.Net.Collector.Interface
{
    public interface IInfluxDbAgentLoader
    {
        IInfluxDbAgent GetAgent(IDatabaseConfig database);
    }
}