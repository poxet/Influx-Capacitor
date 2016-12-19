using System.Threading.Tasks;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Service
{
    internal class ServiceStopListeningCommand : ActionCommandBase
    {
        private readonly ISocketClient _socketClient;

        public ServiceStopListeningCommand(ISocketClient socketClient)
            : base("Mute", "Stop listening for server events.")
        {
            _socketClient = socketClient;
        }

        public override bool CanExecute()
        {
            return _socketClient.IsConnected && _socketClient.IsListening;
        }

        public override async Task<bool> InvokeAsync(string paramList)
        {
            _socketClient.StopListening();
            return true;
        }
    }
}