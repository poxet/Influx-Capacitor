using System;
using System.Threading.Tasks;
using InfluxDB.Net.Contracts;
using InfluxDB.Net.Enums;
using InfluxDB.Net.Infrastructure.Influx;
using InfluxDB.Net.Models;

namespace Tharga.InfluxCapacitor.Interface
{
    public interface ISendResponse
    {
        string StatusCode { get; }
        string Body { get; }
    }

    public interface ISenderAgent
    {
        string TargetDescription { get; }
        Task<ISendResponse> SendAsync(Point[] points);
        string PointToString(Point point);
    }

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