namespace InfluxDB.Net.Collector.Console.Entities
{
    public class Config
    {
        private readonly DatabaseConfig _database;

        public Config(DatabaseConfig database)
        {
            _database = database;
        }

        public DatabaseConfig Database { get { return _database; } }
    }
}