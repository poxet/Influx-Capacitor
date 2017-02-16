using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using InfluxDB.Net;
using InfluxDB.Net.Enums;
using InfluxDB.Net.Infrastructure.Influx;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Interface;
using IFormatter = InfluxDB.Net.Contracts.IFormatter;

namespace Tharga.InfluxCapacitor.Agents
{
    [ExcludeFromCodeCoverage]
    public class InfluxDbAgent : IInfluxDbAgent
    {
        private readonly string _databaseName;
        private readonly Lazy<InfluxDb> _influxDb;
        private readonly string _password;
        private readonly string _url;
        private readonly string _userName;

        public InfluxDbAgent(string url, string databaseName, string userName, string password, TimeSpan? requestTimeout = null, InfluxVersion influxVersion = InfluxVersion.Auto)
        {
            _url = url;
            _databaseName = databaseName;
            _userName = userName;
            _password = password;

            _influxDb = new Lazy<InfluxDb>(() =>
            {
                Uri result;
                if (!Uri.TryCreate(_url, UriKind.Absolute, out result))
                {
                    var exp = new InvalidOperationException("Unable to parse provided connection as url.");
                    exp.Data.Add("Url", _url);
                    throw exp;
                }

                try
                {
                    return new InfluxDb(url, userName, password, influxVersion, requestTimeout);
                }
                catch (Exception exception)
                {
                    var exp = new InvalidOperationException("Could not establish a connection to the database.", exception);
                    exp.Data.Add("Url", _url);
                    exp.Data.Add("DatabaseName", _databaseName);
                    exp.Data.Add("Username", _userName);
                    throw exp;
                }
            });
        }

        private InfluxDb InfluxDb => _influxDb.Value;

        public async Task<InfluxDbApiResponse> WriteAsync(Point[] points)
        {
            return await InfluxDb.WriteAsync(_databaseName, points);
        }

        public async Task CreateDatabaseAsync(string databaseName)
        {
            await InfluxDb.CreateDatabaseAsync(databaseName);
        }

        public Tuple<IFormatter, InfluxVersion> GetAgentInfo()
        {
            return new Tuple<IFormatter, InfluxVersion>(InfluxDb.GetFormatter(), InfluxDb.GetClientVersion());
        }

        public IFormatter GetFormatter()
        {
            return InfluxDb.GetFormatter();
        }

        public string Description => _url + " " + _databaseName;

        public async Task<bool> CanConnect()
        {
            var pong = await InfluxDb.PingAsync();
            return pong.Success;
        }

        public async Task<Pong> PingAsync()
        {
            return await InfluxDb.PingAsync();
        }

        public async Task<string> VersionAsync()
        {
            var pong = await InfluxDb.PingAsync();
            return pong.Version;
        }

        public async Task<InfluxDbApiResponse> AuthenticateDatabaseUserAsync()
        {
            return await InfluxDb.AuthenticateDatabaseUserAsync(_databaseName, _userName, _password);
        }
    }
}