using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tharga.InfluxCapacitor.Console
{
    internal class SocketClient : ISocketClient
    {
        private CancellationTokenSource _cancellationTokenSource;
        private TcpClient _clientSocket;
        private bool _listening;
        private NetworkStream _networkStream;

        public bool IsConnected => _clientSocket != null && _clientSocket.Connected;
        public bool IsListening => _listening;

        public async Task OpenAsync(string address, int port)
        {
            if (_clientSocket != null && _clientSocket.Connected)
                throw new InvalidOperationException("Already connected to server.");

            _clientSocket = new TcpClient();

            await _clientSocket.ConnectAsync(address, port);

            _networkStream = _clientSocket.GetStream();
            _networkStream.ReadTimeout = 10;
            _networkStream.WriteTimeout = 10;

            var returndata = ReadFromServer();

            System.Diagnostics.Debug.WriteLine(returndata);
        }

        private string ReadFromServer()
        {
            var inStream = new byte[_clientSocket.ReceiveBufferSize];
            _networkStream.Read(inStream, 0, _clientSocket.ReceiveBufferSize);
            var returndata = Encoding.ASCII.GetString(inStream);
            returndata = returndata.Substring(0, returndata.IndexOf(Environment.NewLine, StringComparison.Ordinal));
            return returndata;
        }

        public async Task SendAsync(string command)
        {
            if (_clientSocket == null || !_clientSocket.Connected)
                throw new InvalidOperationException("Not connected to server.");

            if (_listening)
                throw new InvalidOperationException("The client is in listening mode and cannot send commands. Stop listening mode to send commands.");

            SendToServer(command);
            //return await Task.Run(() =>
            //{
            //    SendToServer(command);

            //    ////NOTE: Read response from server
            //    //var bytesFrom = new byte[_clientSocket.ReceiveBufferSize];
            //    //_serverStream.Read(bytesFrom, 0, _clientSocket.ReceiveBufferSize);
            //    //var dataFromClient = Encoding.ASCII.GetString(bytesFrom);
            //    //dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf(Environment.NewLine, StringComparison.Ordinal));
            //    //return dataFromClient;
            //});
        }

        public async Task CloseAsync()
        {
            if (_clientSocket == null || !_clientSocket.Connected)
                throw new InvalidOperationException("Not connected to server.");

            await Task.Run(() =>
            {
                _cancellationTokenSource.Cancel();
                _clientSocket.Close();
            });
        }

        //public void StartListening(Action<string> messageCallback)
        //{
        //    if (_clientSocket == null || !_clientSocket.Connected)
        //        throw new InvalidOperationException("Not connected to server.");

        //    if (_listening)
        //        throw new InvalidOperationException("Already listening.");

        //    _listening = true;
        //    _cancellationTokenSource = new CancellationTokenSource();

        //    Task.Run(() =>
        //    {
        //        try
        //        {
        //            //NOTE: Send command to server
        //            var serverStream = _clientSocket.GetStream();
        //            SendToServer("Listen", serverStream);

        //            //NOTE: Read response from server
        //            var running = true;
        //            while (running && !_cancellationTokenSource.IsCancellationRequested)
        //            {
        //                var bytesFrom = new byte[_clientSocket.ReceiveBufferSize];
        //                serverStream.Read(bytesFrom, 0, _clientSocket.ReceiveBufferSize);
        //                var dataFromClient = Encoding.ASCII.GetString(bytesFrom);
        //                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf(Environment.NewLine, StringComparison.Ordinal));

        //                switch (dataFromClient.ToLower())
        //                {
        //                    case "events stopped":
        //                        running = false;
        //                        break;
        //                    case "events started":
        //                        break;
        //                    default:
        //                        messageCallback(dataFromClient);
        //                        break;
        //                }
        //            }
        //        }
        //        finally
        //        {
        //            _listening = false;
        //        }
        //    }, _cancellationTokenSource.Token);
        //}

        private void SendToServer(string message)
        {
            if (!message.EndsWith(Environment.NewLine))
                message += Environment.NewLine;

            var outStream = Encoding.ASCII.GetBytes(message);
            _networkStream.Write(outStream, 0, outStream.Length);
            _networkStream.Flush();
        }

        //public void StopListening()
        //{
        //    if (_clientSocket == null || !_clientSocket.Connected)
        //        throw new InvalidOperationException("Not connected to server.");

        //    if (!_listening)
        //        throw new InvalidOperationException("Not listening.");

        //    var serverStream = _clientSocket.GetStream();
        //    SendToServer("Listen Stop", serverStream);
        //}
    }
}