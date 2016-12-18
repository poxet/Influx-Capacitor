using System;
using Tharga.InfluxCapacitor.Entities;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Collector
{
    public static class OutputLevelExtension
    {
        public static OutputLevel ToOutputLevel(this SendCompleteEventArgs.OutputLevel item)
        {
            switch (item)
            {
                case SendCompleteEventArgs.OutputLevel.Information:
                    return OutputLevel.Information;
                case SendCompleteEventArgs.OutputLevel.Warning:
                    return OutputLevel.Warning;
                case SendCompleteEventArgs.OutputLevel.Error:
                    return OutputLevel.Error;
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Inknown output level {0}.", item));
            }
        }
    }
}