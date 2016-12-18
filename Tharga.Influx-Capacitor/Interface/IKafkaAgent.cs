using System;
using System.Threading.Tasks;
using InfluxDB.Net.Models;

namespace Tharga.InfluxCapacitor.Interface
{
    public interface IKafkaAgent : IDisposable
    {
        Task<IAgentSendResponse> SendAsync(Point[] points);
    }
}