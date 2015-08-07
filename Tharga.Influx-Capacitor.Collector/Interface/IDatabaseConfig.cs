using InfluxDB.Net;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface IDatabaseConfig
    {
        string Url { get; }
        string Username { get; }
        string Password { get; }
        string Name { get; }
        InfluxDbVersion InfluxDbVersion { get; }
    }
}