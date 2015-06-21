namespace InfluxDB.Net.Collector.Console.Entities
{
    public class DatabaseConfig
    {
        private readonly string _url;
        private readonly string _username;
        private readonly string _password;
        private readonly string _name;

        public DatabaseConfig(string url, string username, string password, string name)
        {
            _url = url;
            _username = username;
            _password = password;
            _name = name;
        }

        public string Url { get { return _url; } }
        public string Username { get { return _username; } }
        public string Password { get { return _password; } }
        public string Name { get { return _name; } }
    }
}