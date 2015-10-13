using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class ApplicationConfig : IApplicationConfig
    {
        private readonly int _flushSecondsInterval;
        private readonly bool _debugMode;
        private readonly bool _metadata;
        private readonly int _maxQueueSize;

        public ApplicationConfig(int flushSecondsInterval, bool debugMode, bool metadata, int maxQueueSize)
        {
            _flushSecondsInterval = flushSecondsInterval;
            _debugMode = debugMode;
            _metadata = metadata;
            _maxQueueSize = maxQueueSize;
        }

        public int FlushSecondsInterval { get { return _flushSecondsInterval; } }
        public bool DebugMode { get { return _debugMode; } }
        public bool Metadata { get  { return _metadata; } }
        public int MaxQueueSize { get { return _maxQueueSize; } }
    }
}