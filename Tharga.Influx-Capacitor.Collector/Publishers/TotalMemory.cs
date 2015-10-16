using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Publishers
{
    public class TotalMemory : ICounterPublisher
    {
        #region kernel32

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        #endregion

        private readonly int _secondsInterval;

        public TotalMemory(int secondsInterval)
        {
            _secondsInterval = secondsInterval;
        }

        public string CounterName { get { return "TotalMemory"; } }
        public int SecondsInterval { get { return _secondsInterval;} }
        public string CategoryName { get { return "Influx-Capacitor"; } }
        public string CategoryHelp { get { return "Total memory installed on the machine."; } }
        public PerformanceCounterCategoryType PerformanceCounterCategoryType { get { return PerformanceCounterCategoryType.SingleInstance; } }
        public PerformanceCounterType CounterType { get { return PerformanceCounterType.NumberOfItems64; } }

        public long GetValue()
        {
            ulong installedMemory = 0;
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                installedMemory = memStatus.ullTotalPhys / 1024;
            }

            long result;
            try
            {
                result = Convert.ToInt64(installedMemory);
                //Console.WriteLine("Converted the {0} value {1} to the {2} value {3}.", installedMemory.GetType().Name, installedMemory, result.GetType().Name, result);
            }
            catch (OverflowException exception)
            {
                throw;
                //Console.WriteLine("The {0} value {1} is outside the range of the UInt64 type.", installedMemory.GetType().Name, installedMemory);
            }

            return result;
        }
    }
}