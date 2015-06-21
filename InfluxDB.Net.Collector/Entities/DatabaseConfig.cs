using System;

namespace InfluxDB.Net.Collector.Entities
{
    public class DatabaseConfig
    {
        private readonly string _url;
        private readonly string _username;
        private readonly string _password;
        private readonly string _name;

        public DatabaseConfig(string url, string username, string password, string name)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url", "No url provided.");
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException("username", "No username provided.");
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException("password", "No password provided.");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name", "No name provided.");

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