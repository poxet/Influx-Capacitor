namespace Tharga.InfluxCapacitor.Interface
{
    public interface IAgentSendResponse
    {
        bool IsSuccess { get; }
        int StatusCode { get; }
        string StatusName { get; }
        string Body { get; }
    }
}