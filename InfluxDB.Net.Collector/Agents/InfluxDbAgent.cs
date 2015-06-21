using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using InfluxDB.Net.Collector.Interface;
using InfluxDB.Net.Models;

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

    [ExcludeFromCodeCoverage]
    public class InfluxDbAgent : IInfluxDbAgent
    {
        private readonly InfluxDb _influxDb;

        public InfluxDbAgent(IDatabaseConfig database)
        {
            _influxDb = new InfluxDb(database.Url, database.Username, database.Password);
        }

        public async Task<InfluxDbApiResponse> WriteAsync(string databaseName, TimeUnit milliseconds, Serie serie)
        {
            return await _influxDb.WriteAsync(databaseName, milliseconds, serie);
        }

        public async Task<Pong> PingAsync()
        {
            return await _influxDb.PingAsync();
        }

        public async Task<string> VersionAsync()
        {
            return await _influxDb.VersionAsync();
        }
    }
}