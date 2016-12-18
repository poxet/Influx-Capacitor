namespace Tharga.InfluxCapacitor.Interface
{
    public interface ISendResponse
    {
        string StatusCode { get; }
        string Body { get; }
    }
}