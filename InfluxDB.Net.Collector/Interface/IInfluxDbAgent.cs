using System.Threading.Tasks;
using InfluxDB.Net.Models;

namespace InfluxDB.Net.Collector.Interface
{
    public interface IInfluxDbAgentLoader
    {
        IInfluxDbAgent GetAgent(IDatabaseConfig database);
    }

    public interface IInfluxDbAgent
    {
        Task<Pong> PingAsync();
        Task<string> VersionAsync();
        Task<InfluxDbApiResponse> WriteAsync(string databaseName, TimeUnit milliseconds, Serie serie);
    }
}