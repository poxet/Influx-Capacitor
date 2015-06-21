using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using InfluxDB.Net.Collector.Agents;
using InfluxDB.Net.Collector.Business;

namespace InfluxDB.Net.Collector.Service
{
    public class WindowsService : ServiceBase
    {
        private readonly Processor _processor;

        public WindowsService()
        {
            ServiceName = "InfluxDB.Net.Collector";

            if (!EventLog.SourceExists(ServiceName))
                EventLog.CreateEventSource(ServiceName, "Application");

            _processor = new Processor(new ConfigBusiness(new FileLoaderAgent()), new CounterBusiness(), new InfluxDbAgentLoader());

            // These Flags set whether or not to handle that specific
            //  type of event. Set to true if you need it, false otherwise.
            CanHandlePowerEvent = true;
            CanHandleSessionChangeEvent = true;
            CanPauseAndContinue = true;
            CanShutdown = true;
            CanStop = true;
        }

        static void Main()
        {
            Run(new WindowsService());
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                var configFiles = GetConfigFiles();
                if (!_processor.RunAsync(configFiles).Wait(5000))
                    throw new InvalidOperationException("Cannot start service.");

                base.OnStart(args);
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry(ServiceName, exception.Message, EventLogEntryType.Error);
                throw;
            }
        }

        private string[] GetConfigFiles()
        {
            var configFile = System.Configuration.ConfigurationManager.AppSettings["ConfigFile"];

            string[] configFiles;
            if (!string.IsNullOrEmpty(configFile))
            {
                configFiles = configFile.Split(';');
            }
            else
            {
                var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                configFiles = Directory.GetFiles(currentDirectory, "*.xml");
            }

            EventLog.WriteEntry(ServiceName, "Loading config files: " + string.Join(", ", configFiles));

            return configFiles;
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnContinue()
        {
            base.OnContinue();
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        protected override void OnCustomCommand(int command)
        {
            base.OnCustomCommand(command);
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return base.OnPowerEvent(powerStatus);
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            base.OnSessionChange(changeDescription);
        }
    }
}