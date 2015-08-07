using System;
using System.Diagnostics.CodeAnalysis;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Agents
{
    [ExcludeFromCodeCoverage]
    public class InfluxDbAgentLoader : IInfluxDbAgentLoader
    {
        public IInfluxDbAgent GetAgent(IDatabaseConfig database)
        {
            if (database == null) throw new ArgumentNullException("database", "No database configuration provided.");

            return new InfluxDbAgent(database);
        }
    }
}