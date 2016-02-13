namespace Tharga.Influx_Capacitor.Entities
{
    public class SendResponse
    {
        public SendResponse(string message, double? elapsed)
        {
            Message = message;
            Elapsed = elapsed;
        }

        public string Message { get; private set; }
        public double? Elapsed { get; private set; }
    }
}