using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using InfluxDB.Net.Collector.Interface;
using InfluxDB.Net.Models;

namespace InfluxDB.Net.Collector.Agents
{
    [ExcludeFromCodeCoverage]
    public class InfluxDbAgent : IInfluxDbAgent
    {
        private readonly InfluxDb _influxDb;
        private readonly IDatabaseConfig _databaseConfig;

        public InfluxDbAgent(IDatabaseConfig databaseConfig)
        {
            _databaseConfig = databaseConfig;
            _influxDb = new InfluxDb(_databaseConfig.Url, _databaseConfig.Username, _databaseConfig.Password, _databaseConfig.InfluxDbVersion);
        }

        public async Task<InfluxDbApiResponse> WriteAsync(TimeUnit milliseconds, Serie serie)
        {
            return await _influxDb.WriteAsync(_databaseConfig.Name, milliseconds, serie);
        }

        public async Task<InfluxDbApiResponse> AuthenticateDatabaseUserAsync()
        {
            return await _influxDb.AuthenticateDatabaseUserAsync(_databaseConfig.Name, _databaseConfig.Username, _databaseConfig.Password);
        }

        public async Task<bool> CanConnect()
        {
            var pong = await _influxDb.PingAsync();
            if (pong.Status == "ok")
                return true;
            return false;
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