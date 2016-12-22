//using System.Threading.Tasks;
//using Tharga.Toolkit.Console.Command.Base;

//namespace Tharga.InfluxCapacitor.Console.Commands.Service
//{
//    internal class ServiceStartListeningCommand : ActionCommandBase
//    {
//        private readonly ISocketClient _socketClient;

//        public ServiceStartListeningCommand(ISocketClient socketClient)
//            : base("Listen", "Listen to events from the server.")
//        {
//            _socketClient = socketClient;
//        }

//        public override bool CanExecute()
//        {
//            return _socketClient.IsConnected && !_socketClient.IsListening;
//        }

//        public override async Task<bool> InvokeAsync(string paramList)
//        {
//            _socketClient.StartListening(message => { OutputInformation(message); });
//            return true;
//        }
//    }
//}