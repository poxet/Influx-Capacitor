using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Service
{
    internal class SocketServer : IDisposable
    {
        private readonly RootCommandBase _rootCommandBase;
        private TcpListener _serverSocket;
        private TcpClient _clientSocket;
        private TcpListener _tcpListener;
        private NetworkStream _networkStream;
        private bool _running;

        public SocketServer(RootCommandBase rootCommandBase, SystemConsoleBase systemConsoleBase)
        {
            _rootCommandBase = rootCommandBase;
            systemConsoleBase.LineWrittenEvent += _systemConsoleBase_LineWrittenEvent;
        }

        private void _systemConsoleBase_LineWrittenEvent(object sender, LineWrittenEventArgs e)
        {
            SendToClient(e.Value);
        }

        public void Stop()
        {
            _running = false;
        }

        private void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            _tcpListener = (TcpListener)ar.AsyncState;
            _clientSocket = _tcpListener.EndAcceptTcpClient(ar);
            _networkStream = _clientSocket.GetStream();

            _networkStream.ReadTimeout = 10;
            _networkStream.WriteTimeout = 10;

            //TODO: Respond with a handshake message.
            System.Console.WriteLine("Client connected to service via TCP.");
            var handshakeMessage = "HANDSHAKE" + Environment.NewLine;
            var handshakeBytes = Encoding.ASCII.GetBytes(handshakeMessage);
            _networkStream.Write(handshakeBytes, 0, handshakeBytes.Length);
            _networkStream.Flush();

            _running = true;
            while (_running)
            {
                try
                {
                    var dataFromClient = ReadFromClient();
                    _rootCommandBase.Execute(dataFromClient);
                }
                catch (IOException exception)
                {
                    System.Console.WriteLine(exception.Message);
                    break;
                }
                catch (Exception exception)
                {
                    System.Console.WriteLine(exception.Message);
                    break;
                }
                finally
                {
                    _networkStream?.Dispose();
                }
            }

            System.Console.WriteLine("Client disconnected from service.");
            _serverSocket.BeginAcceptTcpClient(DoAcceptTcpClientCallback, _serverSocket);
        }

        private string ReadFromClient()
        {
            try
            {
                var bytesFrom = new byte[_clientSocket.ReceiveBufferSize];
                _networkStream.Read(bytesFrom, 0, _clientSocket.ReceiveBufferSize);
                var dataFromClient = Encoding.ASCII.GetString(bytesFrom);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf(Environment.NewLine, StringComparison.Ordinal));
                return dataFromClient;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                throw;
            }
        }

        private void SendToClient(string message)
        {
            if (!message.EndsWith(Environment.NewLine))
                message += Environment.NewLine;

            var sendBytes = Encoding.ASCII.GetBytes(message);
            _networkStream?.Write(sendBytes, 0, sendBytes.Length);
            _networkStream?.Flush();
        }

        public void Start(int port)
        {
            _serverSocket = new TcpListener(port);
            _serverSocket.Start();
            var result = _serverSocket.BeginAcceptTcpClient(DoAcceptTcpClientCallback, _serverSocket);
        }

        public void Dispose()
        {
            _networkStream?.Dispose();
        }
    }
}