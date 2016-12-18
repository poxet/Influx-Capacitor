using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using InfluxDB.Net;
using InfluxDB.Net.Contracts;
using InfluxDB.Net.Enums;
using InfluxDB.Net.Models;
using KafkaNet;
using KafkaNet.Model;
using KafkaNet.Protocol;
using Tharga.InfluxCapacitor.Entities;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Agents
{
    [ExcludeFromCodeCoverage]
    public class KafkaAgent : IKafkaAgent
    {
        private readonly IFormatter _formatter;
        private readonly BrokerRouter _router;

        public KafkaAgent(Uri[] kafkaServers)
        {
            var options = new KafkaOptions(kafkaServers);
            _router = new BrokerRouter(options);

            //NOTE: Get a formatter for infludb
            //TODO: Make it possible to send "InfluxVersion.Latest" to get the latest formatter (not Auto).
            var influxDbClient = new InfluxDb("http://localhost", "reapadda", "qwerty", InfluxVersion.v09x);
            _formatter = influxDbClient.GetFormatter();
        }

        public void Dispose()
        {
            _router.Dispose();
        }

        public async Task<IAgentSendResponse> SendAsync(Point[] points)
        {
            using (var client = new Producer(_router))
            {
                var messages = points.Select(x =>
                {
                    var pointToString = _formatter.PointToString(x);
                    return new Message(pointToString);
                }).ToArray();

                try
                {
                    var result = await client.SendMessageAsync("InfluxCapacitor", messages);
                }
                catch (Exception exception)
                {
                    return new AgentSendResponse(HttpStatusCode.InternalServerError, exception.Message);
                }
            }
            return new AgentSendResponse(HttpStatusCode.OK, string.Empty);
        }
    }
}