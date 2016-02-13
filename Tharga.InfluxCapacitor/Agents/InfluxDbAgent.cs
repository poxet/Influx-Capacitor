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
        private readonly string _userName;
        private readonly string _password;
        private readonly InfluxDb _influxDb;

        public InfluxDbAgent(string url, string databaseName, string userName, string password)
        {
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
                _influxDb = new InfluxDb(url, userName, password, InfluxVersion.Auto);
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

        public async Task<InfluxDbApiResponse> AuthenticateDatabaseUserAsync()
        {
            return await _influxDb.AuthenticateDatabaseUserAsync(_databaseName, _userName, _password);
        }

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
    }
}