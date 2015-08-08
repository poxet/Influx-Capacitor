using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using InfluxDB.Net;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Config
{
    abstract class ConfigCommandBase : ActionCommandBase
    {
        private readonly IInfluxDbAgentLoader _influxDbAgentLoader;
        private readonly IConfigBusiness _configBusiness;

        protected ConfigCommandBase(string name, string description, IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness)
            : base(name, description)
        {
            _influxDbAgentLoader = influxDbAgentLoader;
            _configBusiness = configBusiness;
        }

        protected async Task<Tuple<string, InfluxDbVersion>> GetServerUrlAsync(string paramList, int index, string defaultUrl, InfluxDbVersion influxDbVersion)
        {
            var urlParam = GetParam(paramList, index++);
            //var versionParam = GetParam(paramList, index++);

            var url = defaultUrl;

            IInfluxDbAgent client = null;
            if (!string.IsNullOrEmpty(url) && url != Constants.NoConfigUrl)
            {                
                client = _influxDbAgentLoader.GetAgent(new DatabaseConfig(url, "root", "qwerty", "qwerty", influxDbVersion));
            }

            var connectionConfirmed = false;
            try
            {
                if (client != null)
                {
                    connectionConfirmed = await client.CanConnect();
                }
            }
            catch (Exception exception)
            {
                OutputError("{0}", exception.Message);
            }

            if (!connectionConfirmed)
            {
                OutputInformation("Enter the url to the InfluxDB to use.");
                OutputInformation("Provide the correct port, typically 8086. (Ex. http://tharga.net:8086)");
                while (!connectionConfirmed)
                {
                    try
                    {
                        url = QueryParam<string>("Url", urlParam);
                        urlParam = null;
                        //influxDbVersion = QueryParam("Version", versionParam, new Dictionary<InfluxDbVersion, string> { { InfluxDbVersion.Ver0_8X, "0.8x" }, { InfluxDbVersion.Ver0_9X, "0.9x" }, { InfluxDbVersion.Auto, "Auto" } });
                        //versionParam = null;
                        influxDbVersion = InfluxDbVersion.Auto;
                        client = _influxDbAgentLoader.GetAgent(new DatabaseConfig(url, "root", "qwerty", "qwert", influxDbVersion));

                        connectionConfirmed = await client.CanConnect();
                    }
                    catch (CommandEscapeException)
                    {
                        return null;
                    }
                    catch (Exception exception)
                    {
                        OutputError("{0}", exception.Message.Split('\n')[0]);
                    }
                }

                _configBusiness.SaveDatabaseUrl(url, influxDbVersion);
            }
            OutputInformation("Connection to server {0} confirmed.", url);
            return new Tuple<string, InfluxDbVersion>(url, influxDbVersion);
        }

        protected async Task<IDatabaseConfig> GetUsernameAsync(string paramList, int index, IDatabaseConfig config)
        {
            var points = new[]
            {
                new Point
                {
                    Name = Constants.ServiceName, 
                    Tags = new Dictionary<string, object>
                    {
                        { "hostname", Environment.MachineName }
                    },
                    Fields = new Dictionary<string, object>
                    {
                        { "value", 1 }
                    },
                    Precision = TimeUnit.Microseconds,
                    Timestamp = DateTime.UtcNow
                },
            };
            var dataChanged = false;

            var url = config.Url;
            var influxDbVersion = config.InfluxDbVersion;

            IInfluxDbAgent client = null;
            InfluxDbApiResponse response = null;
            try
            {
                if (!string.IsNullOrEmpty(config.Name) && !string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
                {
                    client = _influxDbAgentLoader.GetAgent(config);
                    response = await client.WriteAsync(points);
                }
            }
            catch (Exception exception)
            {
                OutputError(exception.Message);
            }

            if (response == null || !response.Success)
                OutputInformation("Enter the database, username and password for the InfluxDB.");

            while (response == null || !response.Success)
            {
                var database = string.Empty;
                try
                {
                    database = QueryParam<string>("DatabaseName", GetParam(paramList, index++));
                    var user = QueryParam<string>("Username", GetParam(paramList, index++));
                    var password = QueryParam<string>("Password", GetParam(paramList, index++));
                    config = new DatabaseConfig(url, user, password, database, influxDbVersion);

                    client = _influxDbAgentLoader.GetAgent(config);
                    response = await client.WriteAsync(points);
                    dataChanged = true;
                }
                catch (CommandEscapeException)
                {
                    return null;
                }
                catch (InfluxDbApiException exception)
                {
                    if (exception.StatusCode == HttpStatusCode.NotFound)
                    {
                        var create = QueryParam("Database does not exist, create?", GetParam(paramList, index++), new Dictionary<bool, string>() { { true, "Yes" }, { false, "No" } });
                        if (create)
                        {
                            client.CreateDatabaseAsync(database);
                            response = client.WriteAsync(points).Result;
                            dataChanged = true;
                        }
                    }
                }
                catch (Exception exception)
                {
                    OutputError("{0}", exception.Message);
                }
            }

            OutputInformation("Access to database {0} confirmed.", config.Name);

            if (dataChanged)
                _configBusiness.SaveDatabaseConfig(config.Name, config.Username, config.Password);

            return config;
        }
    }
}