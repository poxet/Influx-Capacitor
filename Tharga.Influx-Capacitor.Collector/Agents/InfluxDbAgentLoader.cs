using System.Diagnostics.CodeAnalysis;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Agents
{
    [ExcludeFromCodeCoverage]
    public class InfluxDbAgentLoader : IInfluxDbAgentLoader
    {
        public IInfluxDbAgent GetAgent(IDatabaseConfig database)
        {
            if (database == null)
            {
                return null;
            }

            return new InfluxDbAgent(database);
        }
    }
}