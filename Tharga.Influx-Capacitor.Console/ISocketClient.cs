using System.Threading.Tasks;

namespace Tharga.InfluxCapacitor.Console
{
    public interface ISocketClient
    {
        bool IsConnected { get; }
        bool IsListening { get; }
        Task OpenAsync(string address, int port);
        Task CloseAsync();
        Task SendAsync(string command);
        //void StartListening(Action<string> messageCallback);
        //void StopListening();
    }
}