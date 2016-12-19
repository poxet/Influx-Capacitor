using System.Collections.Generic;
using System.Threading.Tasks;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Service
{
    internal class ServiceConnectCommand : ActionCommandBase
    {
        private readonly ISocketClient _socketClient;

        public ServiceConnectCommand(ISocketClient socketClient)
            : base("Connect", "Connect to service using TCP.")
        {
            _socketClient = socketClient;
        }

        public override bool CanExecute()
        {
            return !_socketClient.IsConnected;
        }

        public override async Task<bool> InvokeAsync(string paramList)
        {
            var index = 0;
            var address = QueryParam("Address", GetParam(paramList, index++), new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("localhost", "localhost") });
            var port = QueryParam("Port", GetParam(paramList, index++), new List<KeyValuePair<int, string>> { new KeyValuePair<int, string>(8888, "8888") });

            await _socketClient.OpenAsync(address, port);

            OutputInformation("Connected");

            return true;
        }
    }
}