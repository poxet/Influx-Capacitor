using InfluxDB.Net.Models;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ISendBusiness
    {
        void Enqueue(Point[] points);
    }
}