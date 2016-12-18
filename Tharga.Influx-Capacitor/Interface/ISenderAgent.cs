using System.Threading.Tasks;
using InfluxDB.Net.Models;

namespace Tharga.InfluxCapacitor.Interface
{
    public interface ISenderAgent
    {
        string TargetDescription { get; }
        Task<ISendResponse> SendAsync(Point[] points);
        string PointToString(Point point);
    }
}