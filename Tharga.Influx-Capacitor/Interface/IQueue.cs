using InfluxDB.Net.Models;

namespace Tharga.InfluxCapacitor
{
    public interface IQueue
    {
        void Enqueue(Point[] points);
        IQueueCountInfo GetQueueInfo();
    }
}