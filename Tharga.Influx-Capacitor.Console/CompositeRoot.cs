using System;
using Tharga.InfluxCapacitor.Collector;
using Tharga.InfluxCapacitor.Collector.Agents;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Entities;
using Tharga.InfluxCapacitor.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console
{
    public class CompositeRoot : ICompositeRoot
    {
        public CompositeRoot()
        {
            //Logger = new MyLogger();
            ClientConsole = new ClientConsole();
            InfluxDbAgentLoader = new InfluxDbAgentLoader();
            FileLoaderAgent = new FileLoaderAgent();
            ConfigBusiness = new ConfigBusiness(FileLoaderAgent);
            ConfigBusiness.InvalidConfigEvent += InvalidConfigEvent;
            CounterBusiness = new CounterBusiness();
            PublisherBusiness = new PublisherBusiness();
            SendBusiness = new SendBusiness(ConfigBusiness, InfluxDbAgentLoader, new ConsoleQueueEvents(ClientConsole));
            SendBusiness.SendBusinessEvent += SendBusinessEvent;
            TagLoader = new TagLoader(ConfigBusiness);
        }

        private void SendBusinessEvent(object sender, SendCompleteEventArgs e)
        {
            ClientConsole.WriteLine(e.Message, e.Level.ToOutputLevel(), null);
        }

        private void InvalidConfigEvent(object sender, InvalidConfigEventArgs e)
        {
            ClientConsole.WriteLine(e.Message, OutputLevel.Warning, null);
        }

        //internal MyLogger Logger { get; private set; }
        public IConsole ClientConsole { get; private set; }
        public IInfluxDbAgentLoader InfluxDbAgentLoader { get; private set; }
        public ISendBusiness SendBusiness { get; private set; }
        public ITagLoader TagLoader { get; private set; }
        public IFileLoaderAgent FileLoaderAgent { get; private set; }
        public IConfigBusiness ConfigBusiness { get; private set; }
        public ICounterBusiness CounterBusiness { get; private set; }
        public IPublisherBusiness PublisherBusiness { get; private set; }
    }

    public class ConsoleQueueEvents : IQueueEvents
    {
        private readonly IConsole _console;

        public ConsoleQueueEvents(IConsole console)
        {
            _console = console;
        }

        public void DebugMessageEvent(string message)
        {
            //NOTE: Use this to se debug information
            //_console.WriteLine(message, OutputLevel.Information);
        }

        public void ExceptionEvent(Exception exception)
        {
            _console.WriteLine(exception.Message, OutputLevel.Error);
        }

        public void SendEvent(ISendEventInfo sendCompleteEventArgs)
        {
            _console.WriteLine(sendCompleteEventArgs.Message, ToLevel(sendCompleteEventArgs.Level));
        }

        private OutputLevel ToLevel(SendEventInfo.OutputLevel level)
        {
            switch (level)
            {
                case SendEventInfo.OutputLevel.Error:
                    return OutputLevel.Error;
                case SendEventInfo.OutputLevel.Warning:
                    return OutputLevel.Warning;
                case SendEventInfo.OutputLevel.Information:
                    return OutputLevel.Information;
                default:
                    return OutputLevel.Default;
            }
        }

        public void QueueChangedEvent(IQueueChangeEventInfo queueChangeEventInfo)
        {
            _console.WriteLine(queueChangeEventInfo.Message, OutputLevel.Information);
        }

        public void TimerEvent(ISendResponse sendResponse)
        {
            _console.WriteLine($"{sendResponse.Message} in {sendResponse.Elapsed.TotalMilliseconds:N2}ms.", OutputLevel.Information);
        }
    }
}