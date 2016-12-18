using InfluxDB.Net.Models;

namespace Tharga.InfluxCapacitor.Interface
{
    public interface IQueue
    {
        void Enqueue(Point[] points);
        IQueueCountInfo GetQueueInfo();
    }
}