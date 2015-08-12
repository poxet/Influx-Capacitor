using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class ApplicationConfig : IApplicationConfig
    {
        private readonly int _flushSecondsInterval;
        private readonly bool _debugMode;

        public ApplicationConfig(int flushSecondsInterval, bool debugMode)
        {
            _flushSecondsInterval = flushSecondsInterval;
            _debugMode = debugMode;
        }

        public int FlushSecondsInterval { get { return _flushSecondsInterval; } }

        public bool DebugMode
        {
            get
            {
                return _debugMode;
            }
        }
    }
}