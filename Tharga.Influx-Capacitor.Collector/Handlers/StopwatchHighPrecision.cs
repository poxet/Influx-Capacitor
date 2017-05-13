//using System.Runtime.InteropServices;

//namespace Tharga.InfluxCapacitor.Collector.Handlers
//{
//    internal class StopwatchHighPrecision
//    {
//        private readonly long _frequency;
//        private readonly long _start;
//        private long _segment;

//        [DllImport("Kernel32.dll")]
//        private static extern void QueryPerformanceCounter(ref long ticks);

//        [DllImport("Kernel32.dll")]
//        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

//        public StopwatchHighPrecision()
//        {
//            QueryPerformanceFrequency(out _frequency);
//            QueryPerformanceCounter(ref _start);
//            _segment = _start;
//        }

//        public long ElapsedTotal
//        {
//            get
//            {
//                QueryPerformanceCounter(ref _segment);
//                return (_segment - _start) * 10000000 / _frequency;
//            }
//        }

//        public long ElapsedSegment
//        {
//            get
//            {
//                var last = _segment;
//                QueryPerformanceCounter(ref _segment);
//                return (_segment - last) * 10000000 / _frequency;
//            }
//        }
//    }
//}