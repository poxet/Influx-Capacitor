using System.Net;
using System.Text;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Entities
{
    public class AgentSendResponse : IAgentSendResponse
    {
        public AgentSendResponse(HttpStatusCode httpStatusCode, string body)
        {
            StatusCode = (int)httpStatusCode;
            StatusName = httpStatusCode.ToString();
            Body = body;
        }

        public bool IsSuccess => StatusCode == 200;
        public int StatusCode { get; }
        public string StatusName { get; }
        public string Body { get; }
    }

    public class QueueChangeEventInfo : IQueueChangeEventInfo
    {
        public QueueChangeEventInfo(IQueueCountInfo before, IQueueCountInfo after)
        {
            Before = before;
            After = after;
        }

        public IQueueCountInfo Before { get; }
        public IQueueCountInfo After { get; }

        public string Message
        {
            get
            {
                var sb = new StringBuilder();
                if (Before.QueueCount != After.QueueCount)
                    sb.AppendFormat("Queue changed from {0} to {1} items. ", Before.QueueCount, After.QueueCount);

                if (Before.FailQueueCount != After.FailQueueCount)
                    sb.AppendFormat("Fail queue changed from {0} to {1} items. ", Before.FailQueueCount, After.FailQueueCount);

                var response = sb.ToString().Trim();
                return string.IsNullOrEmpty(response) ? "No queue change" : response;
            }
        }
    }
}