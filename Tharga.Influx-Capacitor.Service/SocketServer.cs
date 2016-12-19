using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tharga.InfluxCapacitor.Service
{
    internal class SocketServer
    {
        private CancellationTokenSource _listenerTokenSource;
        private TcpListener _serverSocket;
        private TcpClient _clientSocket;
        private TcpListener _tcpListener;

        public void Stop()
        {
            _listenerTokenSource.Cancel();
        }

        private void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            _tcpListener = (TcpListener)ar.AsyncState;
            _clientSocket = _tcpListener.EndAcceptTcpClient(ar);
            var networkStream = _clientSocket.GetStream();

            //TODO: Respond with a handshake message.
            System.Console.WriteLine("Client connected to service via TCP.");
            var handshakeMessage = "HANDSHAKE" + Environment.NewLine;
            var handshakeBytes = Encoding.ASCII.GetBytes(handshakeMessage);
            networkStream.Write(handshakeBytes, 0, handshakeBytes.Length);
            networkStream.Flush();

            while (true)
            {
                try
                {
                    //NOTE: Listening for commands from the client
                    var bytesFrom = new byte[_clientSocket.ReceiveBufferSize];
                    networkStream.Read(bytesFrom, 0, _clientSocket.ReceiveBufferSize);
                    var dataFromClient = Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf(Environment.NewLine, StringComparison.Ordinal));

                    switch (dataFromClient.ToLower())
                    {
                        case "listen":
                            SendToClient("Events started", networkStream);
                            _listenerTokenSource = new CancellationTokenSource();
                            Task.Run(() =>
                            {
                                try
                                {
                                    while (true)
                                    {
                                        SendToClient("Blah", networkStream);
                                        Thread.Sleep(3000);
                                    }
                                }
                                finally
                                {
                                }
                            }, _listenerTokenSource.Token);
                            break;
                        case "listen stop":
                            _listenerTokenSource.Cancel();
                            SendToClient("Events stopped", networkStream);
                            break;
                        default:
                            //TODO: Perform the command and collect the response
                            System.Console.WriteLine("Execute message from client '" + dataFromClient + "'.");

                            //NOTE: Send response to the server
                            SendToClient("RESPONSE_TO_COMMAND " + dataFromClient, networkStream);
                            break;
                    }
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
            }

            System.Console.WriteLine("Client disconnected from service.");
            _serverSocket.BeginAcceptTcpClient(DoAcceptTcpClientCallback, _serverSocket);
        }

        private static void SendToClient(string message, NetworkStream networkStream)
        {
            if (!message.EndsWith(Environment.NewLine))
                message += Environment.NewLine;

            var sendBytes = Encoding.ASCII.GetBytes(message);
            networkStream.Write(sendBytes, 0, sendBytes.Length);
            networkStream.Flush();
        }

        public void Start(int port)
        {
            _serverSocket = new TcpListener(port);
            _serverSocket.Start();
            var result = _serverSocket.BeginAcceptTcpClient(DoAcceptTcpClientCallback, _serverSocket);
        }
    }
}