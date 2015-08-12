using Tharga.InfluxCapacitor.Collector.Agents;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;
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
            SendBusiness = new SendBusiness(ConfigBusiness, InfluxDbAgentLoader);
            SendBusiness.SendBusinessEvent += SendBusinessEvent;
        }

        void SendBusinessEvent(object sender, SendBusinessEventArgs e)
        {
            ClientConsole.WriteLine(e.Message, e.Success ? OutputLevel.Information : (e.Warning ? OutputLevel.Warning : OutputLevel.Error));
        }

        void InvalidConfigEvent(object sender, InvalidConfigEventArgs e)
        {
            ClientConsole.WriteLine(e.Message, OutputLevel.Warning);
        }

        public IConsole ClientConsole { get; private set; }
        public IInfluxDbAgentLoader InfluxDbAgentLoader { get; private set; }
        public ISendBusiness SendBusiness { get; private set; }
        public IFileLoaderAgent FileLoaderAgent { get; private set; }
        public IConfigBusiness ConfigBusiness { get; private set; }
        public ICounterBusiness CounterBusiness { get; private set; }
    }
}