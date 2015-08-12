using System.Collections.Generic;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class Config : IConfig
    {
        private readonly IDatabaseConfig _database;
        private readonly IApplicationConfig _application;
        private readonly List<ICounterGroup> _groups;

        public Config(IDatabaseConfig database, IApplicationConfig application, List<ICounterGroup> groups)
        {
            _database = database;
            _application = application ?? new ApplicationConfig(10, false);
            _groups = groups;
        }

        public IDatabaseConfig Database { get { return _database; } }
        public IApplicationConfig Application { get { return _application; } }
        public List<ICounterGroup> Groups { get { return _groups; } }
    }
}