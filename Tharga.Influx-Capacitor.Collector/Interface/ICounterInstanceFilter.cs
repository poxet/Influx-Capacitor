using Tharga.InfluxCapacitor.Collector.Entities;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    /// <summary>
    /// Defines a filter which can be executed on an instance name to change its value or ignore it from the counters to collect.
    /// </summary>
    public interface    ICounterInstanceFilter
    {
        /// <summary>
        /// Executes the filter.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>a new string with the filter applied, <c>null</c> if the <paramref name="input"/> does not match the filter and should be ignored.</returns>
        Naming Execute(Naming input);
    }
}