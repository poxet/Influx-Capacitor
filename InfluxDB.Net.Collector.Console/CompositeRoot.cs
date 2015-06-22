using InfluxDB.Net.Collector.Agents;
using InfluxDB.Net.Collector.Business;
using InfluxDB.Net.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace InfluxDB.Net.Collector.Console
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
        }

        public IConsole ClientConsole { get; private set; }
        public IInfluxDbAgentLoader InfluxDbAgentLoader { get; private set; }
        public IFileLoaderAgent FileLoaderAgent { get; private set; }
        public IConfigBusiness ConfigBusiness { get; private set; }

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