using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using InfluxDB.Net;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class MetaDataBusiness : IMetaDataBusiness
    {
        public static async Task<InfluxDbApiResponse> TestWriteAccess(IInfluxDbAgent client, string action)
        {
            var tags = new Dictionary<string, string>
            {
                { "hostname", Environment.MachineName },
                { "version", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
                { "action", action },
            };

            var fields = new Dictionary<string, object>
            {
                { "value", 1 }
            };

            var points = new[]
            {
                new Point
                    {
                        Name = Constants.ServiceName, 
                        Tags = tags,
                        Fields = fields,
                        Precision = TimeUnit.Milliseconds,
                        Timestamp = DateTime.UtcNow
                    },
            };

            return await client.WriteAsync(points);
        }

        public static Point GetQueueCountPoints(string action, string targetServer, string targetDatabase, int previousQueueCount, int queueCountChange, Tuple<string,double?> response)
        {
            var tags = new Dictionary<string, string>
            {
                { "hostname", Environment.MachineName },
                { "version", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
                { "action", action },
                { "targetServer", targetServer },
                { "targetDatabase", targetDatabase },
            };

            if (!string.IsNullOrEmpty(response.Item1))
            {
                tags.Add("failMessage", response.Item1);
            }

            var fields = new Dictionary<string, object>
            {
                { "value", previousQueueCount + queueCountChange },
                { "change", queueCountChange },
                { "previousQueueCount", previousQueueCount },
            };

            if (response.Item2 != null)
            {
                fields.Add("elapsed", response.Item2);
            }

            var point = new Point
            {
                Name = Constants.ServiceName,
                Tags = tags,
                Fields = fields,
                Precision = TimeUnit.Milliseconds,
                Timestamp = DateTime.UtcNow
            };
            
            return point;
        }

        public static Point GetCollectorPoint(string engineName, int counters, Dictionary<string, long> timeInfo, double elapseOffset)
        {
            throw new NotImplementedException();
        }
    }
}