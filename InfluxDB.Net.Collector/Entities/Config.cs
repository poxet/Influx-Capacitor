using System.Collections.Generic;

namespace InfluxDB.Net.Collector.Entities
{
    public class Config
    {
        private readonly DatabaseConfig _database;
        private readonly List<CounterGroup> _groups;

        public Config(DatabaseConfig database, List<CounterGroup> groups)
        {
            _database = database;
            _groups = groups;
        }

        public DatabaseConfig Database { get { return _database; } }
        public List<CounterGroup> Groups { get { return _groups; } }
    }
}