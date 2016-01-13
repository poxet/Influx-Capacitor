namespace Tharga.InfluxCapacitor.Interface
{
    public interface ISenderConfiguration
    {
        bool IsEnabled { get; }
        string Type { get; }
    }
}