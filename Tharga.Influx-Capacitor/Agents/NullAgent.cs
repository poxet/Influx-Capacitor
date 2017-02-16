using System;
using System.Net;
using System.Threading.Tasks;
using InfluxDB.Net.Contracts;
using InfluxDB.Net.Enums;
using InfluxDB.Net.Infrastructure.Influx;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Agents
{
    public class NullAgent : IInfluxDbAgent
    {
        private const string Version = "1";

        public async Task<bool> CanConnect()
        {
            return true;
        }

        public async Task<Pong> PingAsync()
        {
            return new Pong { ResponseTime = new TimeSpan(), Success = true, Version = Version };
        }

        public async Task<string> VersionAsync()
        {
            return Version;
        }

        public async Task<InfluxDbApiResponse> WriteAsync(Point[] points)
        {
            return new InfluxDbApiDeleteResponse(HttpStatusCode.Accepted, "");
        }

        public async Task CreateDatabaseAsync(string databaseName)
        {
        }

        public Tuple<IFormatter, InfluxVersion> GetAgentInfo()
        {
            return new Tuple<IFormatter, InfluxVersion>(GetFormatter(), InfluxVersion.v0x);
        }

        public IFormatter GetFormatter()
        {
            return new NullFormatter();
        }

        public string Description { get; }
    }
}