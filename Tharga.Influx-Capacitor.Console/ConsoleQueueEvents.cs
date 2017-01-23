using System;
using Tharga.InfluxCapacitor.Entities;
using Tharga.InfluxCapacitor.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console
{
    public class ConsoleQueueEvents : IQueueEvents
    {
        private readonly IConsole _console;

        public ConsoleQueueEvents(IConsole console)
        {
            _console = console;
        }

        public void OnDebugMessageEvent(string message)
        {
            //NOTE: Use this to se debug information
            //_console.WriteLine(message, OutputLevel.Information);
        }

        public void OnExceptionEvent(Exception exception)
        {
            _console.WriteLine(exception.Message, OutputLevel.Error);
        }

        public void OnSendEvent(ISendEventInfo sendCompleteEventArgs)
        {
            _console.WriteLine(sendCompleteEventArgs.Message, ToLevel(sendCompleteEventArgs.Level));
        }

        private OutputLevel ToLevel(SendEventInfo.OutputLevel level)
        {
            switch (level)
            {
                case SendEventInfo.OutputLevel.Error:
                    return OutputLevel.Error;
                case SendEventInfo.OutputLevel.Warning:
                    return OutputLevel.Warning;
                case SendEventInfo.OutputLevel.Information:
                    return OutputLevel.Information;
                default:
                    return OutputLevel.Default;
            }
        }

        public void OnQueueChangedEvent(IQueueChangeEventInfo queueChangeEventInfo)
        {
            _console.WriteLine(queueChangeEventInfo.Message, OutputLevel.Information);
        }

        public void OnTimerEvent(ISendResponse sendResponse)
        {
            _console.WriteLine($"{sendResponse.Message} in {sendResponse.Elapsed.TotalMilliseconds:N2}ms.", OutputLevel.Information);
        }
    }
}