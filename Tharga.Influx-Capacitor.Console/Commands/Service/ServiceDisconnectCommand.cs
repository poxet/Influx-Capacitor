using System.Threading.Tasks;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Service
{
    internal class ServiceDisconnectCommand : ActionCommandBase
    {
        private readonly ISocketClient _socketClient;

        public ServiceDisconnectCommand(ISocketClient socketClient)
            : base("Disconnect", "Disconnect from the service.")
        {
            _socketClient = socketClient;
        }

        public override bool CanExecute()
        {
            return _socketClient.IsConnected;
        }

        public override async Task<bool> InvokeAsync(string paramList)
        {
            await _socketClient.CloseAsync();

            return true;
        }
    }
}