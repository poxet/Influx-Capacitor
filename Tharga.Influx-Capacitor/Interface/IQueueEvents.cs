using System;
using System.Threading.Tasks;
using InfluxDB.Net.Infrastructure.Influx;
using InfluxDB.Net.Models;

namespace Tharga.InfluxCapacitor.Interface
{
    public interface IMetaDataBusiness
    {
        Task<InfluxDbApiResponse> TestWriteAccess(IInfluxDbAgent client, string action);
        Point BuildQueueMetadata(string action, ISendResponse sendResponse, ISenderAgent senderAgent, IQueueCountInfo queueCountInfo);
    }

    public interface IQueueEvents
    {
        void OnDebugMessageEvent(string message);
        void OnExceptionEvent(Exception exception);
        void OnSendEvent(ISendEventInfo sendCompleteEventArgs);
        void OnQueueChangedEvent(IQueueChangeEventInfo queueChangeEventInfo);
        void OnTimerEvent(ISendResponse sendResponse);
        void OnEnqueueEvent(Point[] enqueuedPoints, Point[] providedPoints, string[] validationErrors);
    }
}