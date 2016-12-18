using System.Threading.Tasks;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Entities;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Agents
{
    public class InfluxDbSenderAgent : ISenderAgent
    {
        private readonly IInfluxDbAgent _influxDbAgent;

        public InfluxDbSenderAgent(IInfluxDbAgent influxDbAgent)
        {
            _influxDbAgent = influxDbAgent;
        }

        public string TargetDescription => $"InfluxDb sender ({_influxDbAgent.Description})";

        public async Task<IAgentSendResponse> SendAsync(Point[] points)
        {
            var response = await _influxDbAgent.WriteAsync(points);
            return new AgentSendResponse(response.Success, response.StatusCode, response.Body);
        }

        public string PointToString(Point point)
        {
            var formatter = _influxDbAgent.GetFormatter();
            return formatter.PointToString(point);
        }
    }
}