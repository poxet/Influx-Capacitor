using System.Net;
using System.Threading.Tasks;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Entities;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Agents
{
    public class NullSenderAgent : ISenderAgent
    {
        public string TargetDescription => "null sender";

        public async Task<IAgentSendResponse> SendAsync(Point[] points)
        {
            return new AgentSendResponse(true, HttpStatusCode.OK, null);
        }

        public string PointToString(Point point)
        {
            //TODO: Add more details about the point
            return point?.Measurement ?? string.Empty;
        }
    }
}