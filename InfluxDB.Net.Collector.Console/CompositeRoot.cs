using InfluxDB.Net.Collector.Agents;
using InfluxDB.Net.Collector.Business;
using InfluxDB.Net.Collector.Interface;

namespace InfluxDB.Net.Collector.Console
{
    public class CompositeRoot : ICompositeRoot
    {
        private Processor _processor;

        public CompositeRoot()
        {
            InfluxDbAgentLoader = new InfluxDbAgentLoader();
            FileLoaderAgent = new FileLoaderAgent();
            ConfigBusiness = new ConfigBusiness(FileLoaderAgent);
        }

        public IInfluxDbAgentLoader InfluxDbAgentLoader { get; private set; }
        public IFileLoaderAgent FileLoaderAgent { get; private set; }
        public IConfigBusiness ConfigBusiness { get; private set; }

        public Processor Processor
        {
            get
            {
                if (_processor == null)
                    _processor = new Processor(new ConfigBusiness(FileLoaderAgent), new CounterBusiness(), InfluxDbAgentLoader);
                return _processor;
            }
        }
    }
}