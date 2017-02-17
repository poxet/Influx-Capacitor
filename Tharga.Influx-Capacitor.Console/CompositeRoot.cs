using Tharga.InfluxCapacitor.Collector;
using Tharga.InfluxCapacitor.Collector.Agents;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Entities;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console
{
    public class CompositeRoot : ICompositeRoot
    {
        public CompositeRoot()
        {
            ClientConsole = new ClientConsole();
            InfluxDbAgentLoader = new InfluxDbAgentLoader();
            FileLoaderAgent = new FileLoaderAgent();
            ConfigBusiness = new ConfigBusiness(FileLoaderAgent);
            ConfigBusiness.InvalidConfigEvent += InvalidConfigEvent;
            CounterBusiness = new CounterBusiness();
            PublisherBusiness = new PublisherBusiness();
            SendBusiness = new SendBusiness(ConfigBusiness, //InfluxDbAgentLoader, 
                new ConsoleQueueEvents(ClientConsole));
            //SendBusiness.SendBusinessEvent += SendBusinessEvent;
            TagLoader = new TagLoader(ConfigBusiness);
            SocketClient = new SocketClient();
        }

        private void SendBusinessEvent(object sender, SendCompleteEventArgs e)
        {
            ClientConsole.WriteLine(e.Message, e.Level.ToOutputLevel(), null);
        }

        private void InvalidConfigEvent(object sender, InvalidConfigEventArgs e)
        {
            ClientConsole.WriteLine(e.Message, OutputLevel.Warning, null);
        }

        public IConsole ClientConsole { get; }
        public IInfluxDbAgentLoader InfluxDbAgentLoader { get; }
        public ISendBusiness SendBusiness { get; }
        public ITagLoader TagLoader { get; }
        public IFileLoaderAgent FileLoaderAgent { get; }
        public IConfigBusiness ConfigBusiness { get; }
        public ICounterBusiness CounterBusiness { get; }
        public IPublisherBusiness PublisherBusiness { get; }
        public ISocketClient SocketClient { get; }
    }
}