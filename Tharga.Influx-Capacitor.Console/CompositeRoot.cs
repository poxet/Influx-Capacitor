using Tharga.InfluxCapacitor.Collector;
using Tharga.InfluxCapacitor.Collector.Agents;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Influx_Capacitor;
using Tharga.Influx_Capacitor.Entities;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console
{
    public class CompositeRoot : ICompositeRoot
    {
        public CompositeRoot()
        {
            Logger = new MyLogger();
            ClientConsole = new ClientConsole();
            InfluxDbAgentLoader = new InfluxDbAgentLoader();
            FileLoaderAgent = new FileLoaderAgent();
            ConfigBusiness = new ConfigBusiness(FileLoaderAgent);
            ConfigBusiness.InvalidConfigEvent += InvalidConfigEvent;
            CounterBusiness = new CounterBusiness();
            PublisherBusiness = new PublisherBusiness();
            SendBusiness = new SendBusiness(ConfigBusiness, InfluxDbAgentLoader);
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

        internal MyLogger Logger { get; private set; }
        public IConsole ClientConsole { get; private set; }
        public IInfluxDbAgentLoader InfluxDbAgentLoader { get; private set; }
        public ISendBusiness SendBusiness { get; private set; }
        public ITagLoader TagLoader { get; private set; }
        public IFileLoaderAgent FileLoaderAgent { get; private set; }
        public IConfigBusiness ConfigBusiness { get; private set; }
        public ICounterBusiness CounterBusiness { get; private set; }
        public IPublisherBusiness PublisherBusiness { get; private set; }
    }
}