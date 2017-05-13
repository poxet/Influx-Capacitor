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
            var sendBusiness = new SendBusiness(configBusiness, new ConsoleQueueEvents(_console));
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
            Trace.WriteLine(e.Value, e.Level.ToString());
        }

        public IConsole Console => _console;

        void _processor_EngineActionEvent(object sender, EngineActionEventArgs e)
        {
            _console.WriteLine(e.Message, e.OutputLevel, null);
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