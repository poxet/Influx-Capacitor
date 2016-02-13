using System.Diagnostics;

namespace Tharga.InfluxCapacitor.Collector
{
    public static class PerformanceCounterHelper
    {
        public static PerformanceCounterCategory GetPerformanceCounterCategory(string category, string machineName)
        {
            PerformanceCounterCategory cat;
            if (string.IsNullOrEmpty(machineName))
                cat = new PerformanceCounterCategory(category);
            else
                cat = new PerformanceCounterCategory(category, machineName);
            return cat;
        }
    }
}