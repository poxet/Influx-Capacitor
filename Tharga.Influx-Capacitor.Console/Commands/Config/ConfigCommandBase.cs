using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using InfluxDB.Net;
using InfluxDB.Net.Infrastructure.Influx;
using Tharga.InfluxCapacitor.Collector;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Influx_Capacitor.Interface;
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

        protected async Task<string> GetServerUrlAsync(string paramList, int index, string defaultUrl)
        {
            var urlParam = GetParam(paramList, index++);

            var url = defaultUrl;

            IInfluxDbAgent client = null;
            if (!string.IsNullOrEmpty(url) && url != Constants.NoConfigUrl)
            {
                try
                {
                    client = _influxDbAgentLoader.GetAgent(new InfluxDatabaseConfig(true, url, "root", "qwerty", "qwerty", null));
                }
                catch (Exception exception)
                {
                    OutputWarning(exception.Message);
                }
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
                        client = _influxDbAgentLoader.GetAgent(new InfluxDatabaseConfig(true, url, "root", "qwerty", "qwert", null));

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

                _configBusiness.SaveDatabaseUrl(url);
            }
            OutputInformation("Connection to server {0} confirmed.", url);
            return url;
        }

        protected async Task<IDatabaseConfig> GetUsernameAsync(string paramList, int index, IDatabaseConfig config, string action)
        {            
            var dataChanged = false;

            var url = config.Url;

            IInfluxDbAgent client = null;
            InfluxDbApiResponse response = null;
            try
            {
                if (!string.IsNullOrEmpty(config.Name) && !string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
                {
                    client = _influxDbAgentLoader.GetAgent(config);
                    response = await MetaDataBusiness.TestWriteAccess(client, action);
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
                    config = new InfluxDatabaseConfig(true, url, user, password, database, null);

                    client = _influxDbAgentLoader.GetAgent(config);
                    response = await MetaDataBusiness.TestWriteAccess(client, action);
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
                            response = MetaDataBusiness.TestWriteAccess(client, action).Result;
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