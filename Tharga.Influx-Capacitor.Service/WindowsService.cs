using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using Tharga.InfluxCapacitor.Collector;
using Tharga.InfluxCapacitor.Collector.Agents;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Handlers;
using Tharga.InfluxCapacitor.Console;
using Tharga.InfluxCapacitor.Entities;
using Tharga.Toolkit.Console.Command;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Service
{
    public class WindowsService : ServiceBase
    {
        private readonly Processor _processor;
        private readonly ServerConsole _console;

        public WindowsService()
        {
            _console = new ServerConsole();
            ServiceName = Constants.ServiceName;

            //TODO: Inject before this point
            var configBusiness = new ConfigBusiness(new FileLoaderAgent());
            configBusiness.InvalidConfigEvent += InvalidConfigEvent;
            var influxDbAgentLoader = new InfluxDbAgentLoader();
            var counterBusiness = new CounterBusiness();
            var publisherBusiness = new PublisherBusiness();
            counterBusiness.GetPerformanceCounterEvent += GetPerformanceCounterEvent;
            CounterBusiness.ChangedCurrentCultureEvent += ChangedCurrentCultureEvent;
            var sendBusiness = new SendBusiness(configBusiness, influxDbAgentLoader, new ConsoleQueueEvents(_console));
            sendBusiness.SendBusinessEvent += SendBusinessEvent;
            var tagLoader = new TagLoader(configBusiness);
            _processor = new Processor(configBusiness, counterBusiness, publisherBusiness, sendBusiness, tagLoader);
            _processor.EngineActionEvent += _processor_EngineActionEvent;

            // These Flags set whether or not to handle that specific
            //  type of event. Set to true if you need it, false otherwise.
            CanHandlePowerEvent = true;
            CanHandleSessionChangeEvent = true;
            CanPauseAndContinue = true;
            CanShutdown = true;
            CanStop = true;

            _console.LineWrittenEvent += _console_LineWrittenEvent;
        }

        private void _console_LineWrittenEvent(object sender, LineWrittenEventArgs e)
        {
            //switch (e.Level.ToString())
            //{
            //    case "Default":
            //        _logger.Debug(e.Value);
            //        break;
            //    case "Information":
            //        _logger.Info(e.Value);
            //        break;
            //    case "Warning":
            //        _logger.Warn(e.Value);
            //        break;
            //    case "Error":
            //        _logger.Error(e.Value);
            //        break;
            //    default:
            //        _logger.Error(string.Format("Unknown output level: {0}", e.Level));
            //        _logger.Info(e.Value);
            //        break;
            //}

            Trace.WriteLine(e.Value, e.Level.ToString());
        }

        public IConsole Console => _console;

        void _processor_EngineActionEvent(object sender, EngineActionEventArgs e)
        {
            _console.WriteLine(e.Message, e.OutputLevel, null);
        }

        private void SendBusinessEvent(object sender, SendCompleteEventArgs e)
        {
            _console.WriteLine(e.Message, e.Level.ToOutputLevel(), null);
        }

        private void ChangedCurrentCultureEvent(object sender, ChangedCurrentCultureEventArgs e)
        {
            _console.WriteLine($"Changed culture from {e.PreviousCulture} to {e.NewCulture}.", OutputLevel.Information, null);
        }

        private void GetPerformanceCounterEvent(object sender, GetPerformanceCounterEventArgs e)
        {
            _console.WriteLine(e.Message, OutputLevel.Warning, null);
        }

        private void InvalidConfigEvent(object sender, InvalidConfigEventArgs e)
        {
            _console.WriteLine(e.Message, OutputLevel.Warning, null);
        }

        static void Main()
        {
            Run(new WindowsService());
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                if (!_processor.RunAsync(new string[] { }).Wait(5000))
                {
                    throw new InvalidOperationException("Cannot start service.");
                }

                var message = $"Service {Constants.ServiceName} version {Assembly.GetExecutingAssembly().GetName().Version} started.";
                _console.WriteLine(message, OutputLevel.Information, null);

                //TODO: Read from config
                var rc = new RootCommand(_console);
                var socketServer = new SocketServer(rc, _console);
                socketServer.Start(8888);
                //TODO: Write about the socket service to log

                base.OnStart(args);
            }
            catch (Exception exception)
            {
                _console.WriteLine(exception.Message, OutputLevel.Error, null);
                throw;
            }
        }

        //private void SocketServer()
        //{
        //    TcpListener serverSocket = new TcpListener(8888);
        //    int requestCount = 0;
        //    TcpClient clientSocket = default(TcpClient);
        //    serverSocket.Start();
        //    Console.WriteLine(" >> Server Started", OutputLevel.Information, ConsoleColor.Cyan);
        //    clientSocket = serverSocket.AcceptTcpClient();
        //    Console.WriteLine(" >> Accept connection from client", OutputLevel.Information, ConsoleColor.Cyan);
        //    requestCount = 0;

        //    while ((true))
        //    {
        //        try
        //        {
        //            requestCount = requestCount + 1;
        //            NetworkStream networkStream = clientSocket.GetStream();
        //            byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
        //            networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
        //            string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
        //            dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
        //            Console.WriteLine(" >> Data from client - " + dataFromClient, OutputLevel.Information, ConsoleColor.Cyan);
        //            string serverResponse = "SOME_SERVER_RESPONSE$"; //"Last Message from client" + dataFromClient;
        //            Byte[] sendBytes = Encoding.ASCII.GetBytes(serverResponse);
        //            networkStream.Write(sendBytes, 0, sendBytes.Length);
        //            networkStream.Flush();
        //            Console.WriteLine(" >> " + serverResponse, OutputLevel.Information, ConsoleColor.Cyan);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.ToString(), OutputLevel.Information, ConsoleColor.Cyan);
        //        }
        //    }

        //    clientSocket.Close();
        //    serverSocket.Stop();
        //    Console.WriteLine(" >> exit", OutputLevel.Information, ConsoleColor.Cyan);
        //    Console.ReadLine();
        //}

        //private void StartSocket()
        //{
        //    string host;
        //    int port = 80;

        //    //if (args.Length == 0)
        //        // If no server name is passed as argument to this program,
        //        // use the current host name as the default.
        //        host = Dns.GetHostName();
        //    //else
        //    //    host = args[0];

        //    string result = SocketSendReceive(host, port);
        //    Console.WriteLine(result, OutputLevel.Information);
        //}

        //private static Socket ConnectSocket(string server, int port)
        //{
        //    Socket s = null;
        //    IPHostEntry hostEntry = null;

        //    // Get host related information.
        //    hostEntry = Dns.GetHostEntry(server);

        //    // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
        //    // an exception that occurs when the host IP Address is not compatible with the address family
        //    // (typical in the IPv6 case).
        //    foreach (IPAddress address in hostEntry.AddressList)
        //    {
        //        IPEndPoint ipe = new IPEndPoint(address, port);
        //        Socket tempSocket =
        //            new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        //        tempSocket.Connect(ipe);

        //        if (tempSocket.Connected)
        //        {
        //            s = tempSocket;
        //            break;
        //        }
        //        else
        //        {
        //            continue;
        //        }
        //    }
        //    return s;
        //}

        //// This method requests the home page content for the specified server.
        //private static string SocketSendReceive(string server, int port)
        //{
        //    string request = "GET / HTTP/1.1\r\nHost: " + server +
        //        "\r\nConnection: Close\r\n\r\n";
        //    Byte[] bytesSent = Encoding.ASCII.GetBytes(request);
        //    Byte[] bytesReceived = new Byte[256];

        //    // Create a socket connection with the specified server and port.
        //    Socket s = ConnectSocket(server, port);

        //    if (s == null)
        //        return ("Connection failed");

        //    // Send request to the server.
        //    s.Send(bytesSent, bytesSent.Length, 0);

        //    // Receive the server home page content.
        //    int bytes = 0;
        //    string page = "Default HTML page on " + server + ":\r\n";

        //    // The following will block until te page is transmitted.
        //    do
        //    {
        //        bytes = s.Receive(bytesReceived, bytesReceived.Length, 0);
        //        page = page + Encoding.ASCII.GetString(bytesReceived, 0, bytes);
        //    }
        //    while (bytes > 0);

        //    return page;
        //}

        //private void StartThePipe()
        //{
        //    //Process pipeClient = new Process();

        //    //pipeClient.StartInfo.FileName = "pipeClient.exe";

        //    using (AnonymousPipeServerStream pipeServer =
        //        new AnonymousPipeServerStream(PipeDirection.Out,
        //            HandleInheritability.Inheritable))
        //    {
        //        // Show that anonymous pipes do not support Message mode.
        //        try
        //        {
        //            _console.WriteLine("[SERVER] Setting ReadMode to \"Message\".", OutputLevel.Information);
        //            pipeServer.ReadMode = PipeTransmissionMode.Message;
        //        }
        //        catch (NotSupportedException e)
        //        {
        //            Console.WriteLine($"[SERVER] Exception:\n    {e.Message}",OutputLevel.Information);
        //        }

        //        Console.WriteLine($"[SERVER] Current TransmissionMode: {pipeServer.TransmissionMode}.", OutputLevel.Information);

        //        // Pass the client process a handle to the server.
        //        //pipeClient.StartInfo.Arguments = pipeServer.GetClientHandleAsString();
        //        //pipeClient.StartInfo.UseShellExecute = false;
        //        //pipeClient.Start();

        //        pipeServer.DisposeLocalCopyOfClientHandle();

        //        try
        //        {
        //            using (var sr = new StreamReader(pipeServer))
        //            {
        //                //sr.AutoFlush = true;
        //                // Send a 'sync message' and wait for client to receive it.
        //                var x = sr.ReadToEnd();
        //                //sw.WriteLine("SYNC");
        //                pipeServer.WaitForPipeDrain();
        //                // Send the console input to the client process.
        //                //Console.Write("[SERVER] Enter text: ");
        //                //sw.WriteLine(Console.ReadLine());

        //                Console.WriteLine(x, OutputLevel.Information);
        //            }

        //            // Read user input and send that to the client process.
        //            //using (StreamWriter sw = new StreamWriter(pipeServer))
        //            //{
        //            //    sw.AutoFlush = true;
        //            //    // Send a 'sync message' and wait for client to receive it.
        //            //    sw.WriteLine("SYNC");
        //            //    pipeServer.WaitForPipeDrain();
        //            //    // Send the console input to the client process.
        //            //    Console.Write("[SERVER] Enter text: ");
        //            //    sw.WriteLine(Console.ReadLine());
        //            //}
        //        }
        //        // Catch the IOException that is raised if the pipe is broken
        //        // or disconnected.
        //        catch (IOException e)
        //        {
        //            Console.WriteLine($"[SERVER] Error: {e.Message}", OutputLevel.Information);
        //        }
        //    }
        //}

        protected override void OnStop()
        {
            base.OnStop();
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnContinue()
        {
            base.OnContinue();
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        protected override void OnCustomCommand(int command)
        {
            base.OnCustomCommand(command);
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return base.OnPowerEvent(powerStatus);
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            base.OnSessionChange(changeDescription);
        }

        public void Start(string[] args)
        {
            OnStart(args);
        }

        public void End()
        {
            OnStop();
        }
    }
}