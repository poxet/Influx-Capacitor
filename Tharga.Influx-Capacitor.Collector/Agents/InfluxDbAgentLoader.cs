using System.Diagnostics.CodeAnalysis;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Agents
{
    [ExcludeFromCodeCoverage]
    public class InfluxDbAgentLoader : IInfluxDbAgentLoader
    {
        public IInfluxDbAgent GetAgent(IDatabaseConfig database)
        {
            return new InfluxDbAgent(database);
        }
    }
}