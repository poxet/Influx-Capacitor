using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using InfluxDB.Net.Enums;
using InfluxDB.Net.Infrastructure.Influx;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Entities;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Business
{
    public class MetaDataBusiness : IMetaDataBusiness
    {
        private const string MetaMeasurementName = "Influx-Capacitor-Metadata";

        public async Task<InfluxDbApiResponse> TestWriteAccess(IInfluxDbAgent client, string action)
        {
            var tags = new Dictionary<string, object>
            {
                { "counter", "configuration" },
                { "hostname", Environment.MachineName },
                { "version", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
                { "action", action }
            };

            var fields = new Dictionary<string, object>
            {
                { "value", (decimal)1.0 }
            };

            var points = new[]
            {
                new Point
                {
                    Measurement = MetaMeasurementName,
                    Tags = tags,
                    Fields = fields,
                    Precision = TimeUnit.Milliseconds,
                    Timestamp = DateTime.UtcNow
                }
            };

            return await client.WriteAsync(points);
        }

        public Point BuildQueueMetadata(string action, ISendResponse sendResponse, ISenderAgent senderAgent, IQueueCountInfo queueCountInfo)
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();

            var tags = new Dictionary<string, object>
            {
                { "counter", "queueCount" },
                { "hostname", Environment.MachineName },
                { "version", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
                { "application", assemblyName.FullName },
                { "target", senderAgent.TargetDescription },
                { "message", sendResponse.Message },
                { "isSuccess", sendResponse.IsSuccess },
                { "action", action }
            };            

            var fields = new Dictionary<string, object>
            {
                //{ "value", sendResponse.PointCount },
                { "value", queueCountInfo.TotalQueueCount },
                { "elapsed", sendResponse.Elapsed },
                { "failQueueCount", queueCountInfo.FailQueueCount },
                { "totalQueueCount", queueCountInfo.TotalQueueCount },
                { "queueCount", queueCountInfo.QueueCount },
                { "absoluteAmountDelta", sendResponse.PointCount },
            };

            var point = new Point
            {
                Measurement = MetaMeasurementName,
                Tags = tags,
                Fields = fields,
                Precision = TimeUnit.Milliseconds,
                Timestamp = DateTime.UtcNow
            };

            return point;
        }

        //public Point GetQueueCountPoints(string action, string targetServer, string targetDatabase, int previousQueueCount, int queueCountChange, SendResponse response)
        //{
        //    var tags = new Dictionary<string, object>
        //    {
        //        { "counter", "queueCount" },
        //        { "hostname", Environment.MachineName },
        //        { "version", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
        //        { "action", action },
        //        { "targetServer", targetServer },
        //        { "targetDatabase", targetDatabase }
        //    };
        //
        //    if (!string.IsNullOrEmpty(response.Message))
        //    {
        //        tags.Add("failMessage", response.Message);
        //    }
        //
        //    var fields = new Dictionary<string, object>
        //    {
        //        { "value", (decimal)(previousQueueCount + queueCountChange) },
        //        //{ "queueCount", previousQueueCount + queueCountChange },
        //        { "queueCountChange", queueCountChange }
        //    };
        //
        //    fields.Add("sendTimeMs", response.Elapsed);
        //
        //    var point = new Point
        //    {
        //        Measurement = MetaMeasurementName,
        //        Tags = tags,
        //        Fields = fields,
        //        Precision = TimeUnit.Milliseconds,
        //        Timestamp = DateTime.UtcNow
        //    };
        //
        //    return point;
        //}

        public IEnumerable<Point> GetCollectorPoint(string engineName, string performanceCounterGroup, int counters, Dictionary<string, long> timeInfo, double? elapseOffsetMs)
        {
            var tags = new Dictionary<string, object>
            {
                { "counter", "readCount" },
                { "hostname", Environment.MachineName },
                { "version", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
                { "action", "collect" },
                { "performanceCounterGroup", performanceCounterGroup },
                { "engineName", engineName }
            };

            var fields = new Dictionary<string, object>
            {
                { "value", (decimal)counters }
                //{ "readCount", counters }
            };

            if (elapseOffsetMs != null)
            {
                fields.Add("elapseOffsetTimeMs", (decimal)elapseOffsetMs.Value);
            }

            var totalTimeMs = timeInfo.Sum(x => x.Value);
            fields.Add("totalTimeMs", (decimal)new TimeSpan(totalTimeMs).TotalMilliseconds);

            var now = DateTime.UtcNow;

            yield return new Point
            {
                Measurement = MetaMeasurementName,
                Tags = tags,
                Fields = fields,
                Precision = TimeUnit.Milliseconds,
                Timestamp = now
            };

            var index = 0;
            foreach (var ti in timeInfo)
            {
                index++;

                fields = new Dictionary<string, object>
                {
                    { "value", (decimal)new TimeSpan(ti.Value).TotalMilliseconds }
                    //{ "readTime", (decimal)new TimeSpan(ti.Value).TotalMilliseconds }
                };

                tags = new Dictionary<string, object>
                {
                    { "counter", "readTime" },
                    { "hostname", Environment.MachineName },
                    { "version", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
                    { "action", "collect" },
                    { "performanceCounterGroup", performanceCounterGroup },
                    { "engineName", engineName },
                    { "step", index + "-" + ti.Key }
                };

                yield return new Point
                {
                    Measurement = MetaMeasurementName,
                    Tags = tags,
                    Fields = fields,
                    Precision = TimeUnit.Milliseconds,
                    Timestamp = now
                };
            }
        }
    }
}