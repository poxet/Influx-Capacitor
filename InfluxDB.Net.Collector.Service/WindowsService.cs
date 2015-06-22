using System;
using System.Diagnostics;
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

            //TODO: Inject before this point
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
                if (!_processor.RunAsync(new string[]{}).Wait(5000))
                    throw new InvalidOperationException("Cannot start service.");

                base.OnStart(args);
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry(ServiceName, exception.Message, EventLogEntryType.Error);
                throw;
            }
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