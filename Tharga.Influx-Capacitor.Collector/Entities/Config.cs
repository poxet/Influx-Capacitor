using System.Collections.Generic;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class Config : IConfig
    {
        private readonly List<IDatabaseConfig> _databases;
        private readonly IApplicationConfig _application;
        private readonly List<ICounterGroup> _groups;
        private readonly List<ICounterPublisher> _publishers;
        private readonly List<ITag> _tags;

        public Config(List<IDatabaseConfig> databases, IApplicationConfig application, List<ICounterGroup> groups, List<ICounterPublisher> publishers, List<ITag> tags)
        {
            _databases = databases;
            _application = application ?? new ApplicationConfig(10, false, true, 20000);
            _groups = groups;
            _publishers = publishers;
            _tags = tags;
        }

        public List<IDatabaseConfig> Databases { get { return _databases; } }
        public IApplicationConfig Application { get { return _application; } }
        public List<ICounterGroup> Groups { get { return _groups; } }
        public List<ICounterPublisher> Publishers { get { return _publishers; } }
        public List<ITag> Tags { get { return _tags; } }
    }
}