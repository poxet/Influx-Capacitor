using System;
using System.Threading.Tasks;
using InfluxDB.Net.Contracts;
using InfluxDB.Net.Enums;
using InfluxDB.Net.Infrastructure.Influx;
using InfluxDB.Net.Models;

namespace Tharga.Influx_Capacitor.Interface
{
    public interface IInfluxDbAgent
    {
        Task<bool> CanConnect();
        Task<Pong> PingAsync();
        Task<string> VersionAsync();
        Task<InfluxDbApiResponse> WriteAsync(Point[] points);
        Task CreateDatabaseAsync(string databaseName);
        Tuple<IFormatter, InfluxVersion> GetAgentInfo();
        IFormatter GetFormatter();
    }
}