using System;
using System.Diagnostics;
using System.ServiceProcess;
using Tharga.InfluxCapacitor.Collector;
using Tharga.InfluxCapacitor.Collector.Agents;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Handlers;

namespace Tharga.InfluxCapacitor.Service
{
    public class WindowsService : ServiceBase
    {
        private readonly Processor _processor;

        public WindowsService()
        {
            ServiceName = Constants.ServiceName;

            if (!EventLog.SourceExists(ServiceName))
            {
                EventLog.CreateEventSource(ServiceName, "Application");
            }

            //TODO: Inject before this point
            var configBusiness = new ConfigBusiness(new FileLoaderAgent());
            configBusiness.InvalidConfigEvent += InvalidConfigEvent;
            var influxDbAgentLoader = new InfluxDbAgentLoader();
            var counterBusiness = new CounterBusiness();
            counterBusiness.GetPerformanceCounterEvent += GetPerformanceCounterEvent;
            var sendBusiness = new SendBusiness(configBusiness, influxDbAgentLoader);
            sendBusiness.SendBusinessEvent += SendBusinessEvent;
            _processor = new Processor(configBusiness, counterBusiness, sendBusiness);
            _processor.EngineActionEvent += _processor_EngineActionEvent;

            // These Flags set whether or not to handle that specific
            //  type of event. Set to true if you need it, false otherwise.
            CanHandlePowerEvent = true;
            CanHandleSessionChangeEvent = true;
            CanPauseAndContinue = true;
            CanShutdown = true;
            CanStop = true;
        }

        void _processor_EngineActionEvent(object sender, EngineActionEventArgs e)
        {
            if (!e.Success)
            {
                EventLog.WriteEntry(Constants.ServiceName, e.Message, EventLogEntryType.Error);
            }
        }

        private void SendBusinessEvent(object sender, SendBusinessEventArgs e)
        {
            if (!e.Success)
            {
                EventLog.WriteEntry(Constants.ServiceName, e.Message, e.Warning ? EventLogEntryType.Warning : EventLogEntryType.Error);
            }
        }

        private void GetPerformanceCounterEvent(object sender, GetPerformanceCounterEventArgs e)
        {
            EventLog.WriteEntry(Constants.ServiceName, e.Message, EventLogEntryType.Warning);
        }

        private void InvalidConfigEvent(object sender, InvalidConfigEventArgs e)
        {
            EventLog.WriteEntry(Constants.ServiceName, e.Message, EventLogEntryType.Warning);
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
                if (!_processor.RunAsync(new string[] { }).Wait(5000))
                {
                    throw new InvalidOperationException("Cannot start service.");
                }

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