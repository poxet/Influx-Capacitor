using InfluxDB.Net.Models;

namespace Tharga.InfluxCapacitor
{
    internal class RetryPoint
    {
        public RetryPoint(int retryCount, Point point)
        {
            RetryCount = retryCount;
            Point = point;
        }

        public int RetryCount { get; }
        public Point Point { get; }
    }
}