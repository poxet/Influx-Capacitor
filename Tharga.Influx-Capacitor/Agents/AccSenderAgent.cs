using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Entities;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Agents
{
    public class AccSenderAgent : ISenderAgent
    {
        private readonly int _maxQueueSize;
        private readonly List<Point> _points = new List<Point>();

        public AccSenderAgent()
        {
            _maxQueueSize = 20000;
        }

        public string TargetDescription => $"Acc sender (max {_maxQueueSize})";

        public async Task<IAgentSendResponse> SendAsync(Point[] points)
        {
            if (_maxQueueSize - _points.Count < points.Length)
            {
                return new AgentSendResponse(false, HttpStatusCode.RequestEntityTooLarge, null);
            }

            _points.AddRange(points);

            return new AgentSendResponse(true, HttpStatusCode.OK, null);
        }

        public string PointToString(Point point)
        {
            //TODO: Add more details about the point
            return point.Measurement;
        }
    }
}