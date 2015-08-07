using System.Diagnostics.CodeAnalysis;
using InfluxDB.Net.Collector.Interface;

namespace InfluxDB.Net.Collector.Agents
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