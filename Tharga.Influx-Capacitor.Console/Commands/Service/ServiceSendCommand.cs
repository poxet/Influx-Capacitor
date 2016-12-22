using System.Threading.Tasks;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Service
{
    internal class ServiceSendCommand : ActionCommandBase
    {
        private readonly ISocketClient _socketClient;

        public ServiceSendCommand(ISocketClient socketClient)
            : base("Send", "Send a command to the service.")
        {
            _socketClient = socketClient;
        }

        public override bool CanExecute()
        {
            return _socketClient.IsConnected && !_socketClient.IsListening;
        }

        public override async Task<bool> InvokeAsync(string paramList)
        {
            var index = 0;
            var command = QueryParam<string>("Command", GetParam(paramList, index++));

            await _socketClient.SendAsync(command);

            //OutputInformation(response);

            return true;
        }
    }
}