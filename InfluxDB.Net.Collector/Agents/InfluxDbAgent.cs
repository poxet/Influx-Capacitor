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
            _influxDb = new InfluxDb(_databaseConfig.Url, _databaseConfig.Username, _databaseConfig.Password);
        }

        public async Task<InfluxDbApiResponse> WriteAsync(Point[] points)
        {
            return await _influxDb.WriteAsync(_databaseConfig.Name, points);
        }
        
        public async Task<Pong> PingAsync()
        {
            return await _influxDb.PingAsync();
        }      
    }
}