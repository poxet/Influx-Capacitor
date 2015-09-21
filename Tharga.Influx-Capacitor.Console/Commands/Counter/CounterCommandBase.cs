using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal abstract class CounterCommandBase : ActionCommandBase
    {
        protected CounterCommandBase(string name, string description)
            : base(name, description)
        {
        }

        protected void ReadCounterGroup(IPerformanceCounterGroup counterGroup)
        {
            var count = 0;
            foreach (var counter in counterGroup.GetFreshCounters())
            {
                if (counter.PerformanceCounter != null)
                {
                    var nextValue = counter.PerformanceCounter.NextValue();
                    OutputInformation("{0}.{1}.{2} {3}:\t{4}", counter.PerformanceCounter.CategoryName, counter.PerformanceCounter.CounterName, counter.PerformanceCounter.InstanceName, counter.Name, nextValue);
                    count++;
                }
                else
                {
                    OutputWarning("Cannot read counter {0}.", counter.Name);
                }
            }

            OutputInformation("{0} counters read.", count);
        }
    }
}