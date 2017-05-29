using System.Threading.Tasks;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Agents
{
    public class KafkaSenderAgent : ISenderAgent
    {
        private readonly IKafkaAgent _kafkaAgent;

        public KafkaSenderAgent(IKafkaAgent kafkaAgent)
        {
            _kafkaAgent = kafkaAgent;
        }

        public string TargetDescription => "Kafka sender";

        public async Task<IAgentSendResponse> SendAsync(Point[] points)
        {
            return await _kafkaAgent.SendAsync(points);
        }

        public string PointToString(Point point)
        {
            //TODO: Add more details about the point
            return point?.Measurement ?? string.Empty;
        }
    }
}