using System.Threading.Tasks;
using InfluxDB.Net.Models;

namespace InfluxDB.Net.Collector.Interface
{
    public interface IInfluxDbAgent
    {
        Task<Pong> PingAsync();
        Task<InfluxDbApiResponse> WriteAsync(Point[] points);
    }
}