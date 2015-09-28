using System;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Event;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ICollectorEngine
    {
        Task StartAsync();
        event EventHandler<CollectRegisterCounterValuesEventArgs> CollectRegisterCounterValuesEvent;
        Task<int> CollectRegisterCounterValuesAsync();
    }
}