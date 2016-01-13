using System.Diagnostics.CodeAnalysis;
using Tharga.InfluxCapacitor.Agents;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Interface;

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

            return new InfluxDbAgent(database.Url, database.Name, database.Username, database.Password);
        }
    }
}