using System.Threading.Tasks;
using InfluxDB.Net;
using InfluxDB.Net.Models;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface IInfluxDbAgent
    {
        Task<bool> CanConnect();
        Task<Pong> PingAsync();
        Task<string> VersionAsync();
        Task<InfluxDbApiResponse> WriteAsync(Point[] points);
    }
}