using System;
using System.Linq;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector;
using Tharga.InfluxCapacitor.Collector.Handlers;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Console.Commands.Service;
using Tharga.InfluxCapacitor.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Config
{
    internal class ConfigAutoCommand : ConfigCommandBase
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;

        public ConfigAutoCommand(IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness, ICounterBusiness counterBusiness, IMetaDataBusiness metaDataBusiness)
            : base("Auto", "Automatically run full setup.", influxDbAgentLoader, configBusiness, metaDataBusiness)
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
        }

        public override async Task<bool> InvokeAsync(string paramList)
        {
            var status = true;
            try
            {
                var index = 0;

                var configs = _configBusiness.OpenDatabaseConfig().ToArray();
                if (configs.Length > 1)
                {
                    OutputWarning("There are {0} database targets configured. When using multiple targets you will have to update the config files manually.", configs.Length);
                    return false;
                }

                var defaultUrl = configs.First().Url;

                var response = await GetServerUrlAsync(paramList, index++, defaultUrl);
                if (string.IsNullOrEmpty(response))
                {
                    status = false;
                    return false;
                }

                var config = _configBusiness.OpenDatabaseConfig().First();
                var logonInfo = await GetUsernameAsync(paramList, index++, config, "config_auto");
                if (logonInfo == null)
                {
                    status = false;
                    return false;
                }

                _configBusiness.InitiateApplicationConfig();

                if (!InitiateDefaultCounters())
                {
                    status = false;
                    return false;
                }

                if (!await StartService())
                {
                    status = false;
                    return false;
                }

                return true;
            }
            finally
            {
                if (!status)
                {
                    OutputLine("Setup did not go as it should. Correct the issues manually and type exit when you are done.", ConsoleColor.Blue, OutputLevel.Information);
                    OutputLine("Use the site https://github.com/poxet/Influx-Capacitor for support.", ConsoleColor.Blue, OutputLevel.Information);
                }
            }
        }

        private bool InitiateDefaultCounters()
        {
            var initaiteBusiness = new DataInitiator(_configBusiness, _counterBusiness);
            var messages = initaiteBusiness.CreateAll();
            var someIssue = false;
            foreach (var message in messages)
            {
                OutputLine(message.Item1, message.Item2);

                if (message.Item2 == OutputLevel.Error)
                {
                    someIssue = true;
                }
            }

            return !someIssue;
        }

        private async Task<bool> StartService()
        {
            OutputInformation("Trying to start the service...");

            var result = await ServiceCommands.GetServiceStatusAsync();
            if (result != null)
            {
                await ServiceCommands.RestartServiceAsync();
            }
            else
            {
                OutputError("The service {0} cannot be found on this machine.", Constants.ServiceName);
                return false;
            }

            return true;
        }
    }
}