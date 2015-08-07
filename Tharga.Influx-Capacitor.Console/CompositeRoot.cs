using Tharga.InfluxCapacitor.Collector;
using Tharga.InfluxCapacitor.Collector.Agents;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console
{
    public class CompositeRoot : ICompositeRoot
    {
        private Processor _processor;

        public CompositeRoot()
        {
            ClientConsole = new ClientConsole();
            InfluxDbAgentLoader = new InfluxDbAgentLoader();
            FileLoaderAgent = new FileLoaderAgent();
            ConfigBusiness = new ConfigBusiness(FileLoaderAgent);
            CounterBusiness = new CounterBusiness();
        }

        public IConsole ClientConsole { get; private set; }
        public IInfluxDbAgentLoader InfluxDbAgentLoader { get; private set; }
        public IFileLoaderAgent FileLoaderAgent { get; private set; }
        public IConfigBusiness ConfigBusiness { get; private set; }
        public ICounterBusiness CounterBusiness { get; private set; }

        public Processor Processor
        {
            get
            {
                if (_processor == null)
                {
                    _processor = new Processor(new ConfigBusiness(FileLoaderAgent), new CounterBusiness(), InfluxDbAgentLoader);
                    _processor.NotificationEvent += NotificationEvent;
                }
                return _processor;
            }
        }

        private void NotificationEvent(object sender, NotificationEventArgs e)
        {
            ClientConsole.WriteLine(e.Message, e.OutputLevel);
        }
    }
}