namespace Tharga.InfluxCapacitor.Entities
{
    public class SendResponse
    {
        public SendResponse(string message, double? elapsed)
        {
            Message = message;
            Elapsed = elapsed;
        }

        public string Message { get; }
        public double? Elapsed { get; }
    }
}