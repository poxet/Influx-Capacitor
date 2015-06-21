using System.Collections.Generic;
using InfluxDB.Net.Collector.Interface;

namespace InfluxDB.Net.Collector.Entities
{
    public class Config : IConfig
    {
        private readonly IDatabaseConfig _database;
        private readonly List<ICounterGroup> _groups;

        public Config(IDatabaseConfig database, List<ICounterGroup> groups)
        {
            _database = database;
            _groups = groups;
        }

        public IDatabaseConfig Database { get { return _database; } }
        public List<ICounterGroup> Groups { get { return _groups; } }
    }
}