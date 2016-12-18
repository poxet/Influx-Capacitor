using System.Text;

namespace Tharga.InfluxCapacitor.Entities
{
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