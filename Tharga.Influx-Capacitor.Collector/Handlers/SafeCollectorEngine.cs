using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Net.Enums;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Collector.Handlers
{
    public class SafeCollectorEngine : CollectorEngineBase
    {
        private readonly object _syncRoot = new object();

        public SafeCollectorEngine(IPerformanceCounterGroup performanceCounterGroup, ISendBusiness sendBusiness, ITagLoader tagLoader, bool metadata)
            : base(performanceCounterGroup, sendBusiness, tagLoader, metadata)
        {
        }

        public override async Task<int> CollectRegisterCounterValuesAsync()
        {
            lock (_syncRoot)
            {
                try
                {
                    StopTimer();

                    var swMain = new StopwatchHighPrecision();
                    var timeInfo = new Dictionary<string, long>();

                    if (_timestamp == null)
                    {
                        _timestamp = DateTime.UtcNow;
                        var nw = Floor(_timestamp.Value, new TimeSpan(0, 0, 0, 1));
                        _timestamp = nw;
                    }

                    var timestamp = DateTime.UtcNow;

                    var precision = TimeUnit.Seconds;
                    //No synchronization when running the safe collector engine
                    //timeInfo.Add(TimerConstants.Synchronize, swMain.ElapsedSegment);

                    //TODO: Create a mutex lock here (So that two counters canno read the same signature at the same time, since the content of the _performanceCounterGroup might change during this process.

                    //Prepare read
                    var performanceCounterInfos = PrepareCounters();
                    timeInfo.Add(TimerConstants.Prepare, swMain.ElapsedSegment);

                    //Perform Read (This should be as fast and short as possible)
                    var values = ReadValues(performanceCounterInfos);
                    timeInfo.Add(TimerConstants.Read, swMain.ElapsedSegment);

                    //Prepare result                
                    var points = FormatResult(performanceCounterInfos, values, precision, timestamp).ToArray();
                    timeInfo.Add(TimerConstants.Format, swMain.ElapsedSegment);

                    //Queue result
                    Enqueue(points);
                    timeInfo.Add(TimerConstants.Enque, swMain.ElapsedSegment);

                    //Cleanup
                    RemoveObsoleteCounters(values, performanceCounterInfos);
                    timeInfo.Add(TimerConstants.Cleanup, swMain.ElapsedSegment);

                    if (_metadata)
                    {
                        Enqueue(MetaDataBusiness.GetCollectorPoint(EngineName, Name, points.Length, timeInfo, null).ToArray());
                    }

                    OnCollectRegisterCounterValuesEvent(new CollectRegisterCounterValuesEventArgs(Name, points.Length, timeInfo, 0, OutputLevel.Default));

                    //TODO: Release mutex

                    //TOOD: Send metadata about the read to influx, (this should be configurable)

                    return points.Length;
                }
                catch (Exception exception)
                {
                    OnCollectRegisterCounterValuesEvent(new CollectRegisterCounterValuesEventArgs(Name, exception));
                    return -1;
                }
                finally
                {
                    ResumeTimer();
                }
            }
        }
    }
}