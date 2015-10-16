using System.Diagnostics;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Publishers
{
    public class Random : ICounterPublisher
    {
        private readonly int _secondsInterval;
        private long _value = 100;
        private System.Random _rng = new System.Random();

        public Random(int secondsInterval)
        {
            _secondsInterval = secondsInterval;
        }

        public string CounterName { get { return "Random"; } }
        public int SecondsInterval { get { return _secondsInterval; } }
        public string CategoryName { get { return "Influx-Capacitor"; } }
        public string CategoryHelp { get { return "Random data performance counter."; } }
        public PerformanceCounterCategoryType PerformanceCounterCategoryType { get { return PerformanceCounterCategoryType.SingleInstance; } }
        public PerformanceCounterType CounterType { get { return PerformanceCounterType.NumberOfItems64; } }

        public long GetValue()
        {
            _value += _rng.Next(-10, 10);
            if (_value < 0)
            {
                _value = 0;
            }

            return _value;
        }
    }
}