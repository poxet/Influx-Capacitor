using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using InfluxDB.Net;
using InfluxDB.Net.Contracts;
using InfluxDB.Net.Enums;
using InfluxDB.Net.Infrastructure.Influx;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Agents
{
    [ExcludeFromCodeCoverage]
    public class InfluxDbAgent : IInfluxDbAgent
    {
        private readonly string _databaseName;
        private readonly InfluxDb _influxDb;
        private readonly string _password;
        private readonly string _url;
        private readonly string _userName;

        public InfluxDbAgent(string url, string databaseName, string userName, string password, TimeSpan? requestTimeout, InfluxVersion influxVersion = InfluxVersion.Auto)
        {
            _url = url;
            _databaseName = databaseName;
            _userName = userName;
            _password = password;

            Uri result;
            if (!Uri.TryCreate(url, UriKind.Absolute, out result))
            {
                var exp = new InvalidOperationException("Unable to parse provided connection as url.");
                exp.Data.Add("Url", url);
                throw exp;
            }

            try
            {
                _influxDb = new InfluxDb(url, userName, password, influxVersion, requestTimeout);
            }
            catch (Exception exception)
            {
                var exp = new InvalidOperationException("Could not establish a connection to the database.", exception);
                exp.Data.Add("Url", url);
                exp.Data.Add("Username", userName);
                throw exp;
            }
        }

        public async Task<InfluxDbApiResponse> WriteAsync(Point[] points)
        {
            return await _influxDb.WriteAsync(_databaseName, points);
        }

        public async Task CreateDatabaseAsync(string databaseName)
        {
            await _influxDb.CreateDatabaseAsync(databaseName);
        }

        public Tuple<IFormatter, InfluxVersion> GetAgentInfo()
        {
            return new Tuple<IFormatter, InfluxVersion>(_influxDb.GetFormatter(), _influxDb.GetClientVersion());
        }

        public IFormatter GetFormatter()
        {
            return _influxDb.GetFormatter();
        }

        public string Description => _url + " " + _databaseName;

        public async Task<bool> CanConnect()
        {
            var pong = await _influxDb.PingAsync();
            return pong.Success;
        }

        public async Task<Pong> PingAsync()
        {
            return await _influxDb.PingAsync();
        }

        public async Task<string> VersionAsync()
        {
            var pong = await _influxDb.PingAsync();
            return pong.Version;
        }

        public async Task<InfluxDbApiResponse> AuthenticateDatabaseUserAsync()
        {
            return await _influxDb.AuthenticateDatabaseUserAsync(_databaseName, _userName, _password);
        }
    }
}