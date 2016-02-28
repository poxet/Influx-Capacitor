using System.Diagnostics.CodeAnalysis;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Influx_Capacitor.Agents;
using Tharga.Influx_Capacitor.Interface;

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

            return new InfluxDbAgent(database.Url, database.Name, database.Username, database.Password, database.RequestTimeout);
        }
    }
}