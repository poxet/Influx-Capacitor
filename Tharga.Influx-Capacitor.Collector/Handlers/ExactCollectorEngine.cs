using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Net.Enums;
using Tharga.InfluxCapacitor.Business;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Collector.Handlers
{
    internal class ExactCollectorEngine : CollectorEngineBase
    {
        private readonly object _syncRoot = new object();
        private readonly MetaDataBusiness _metaDataBusiness;
        private StopwatchHighPrecision _sw;
        private long _counter;
        private int _missCounter;

        public ExactCollectorEngine(IPerformanceCounterGroup performanceCounterGroup, ISendBusiness sendBusiness, ITagLoader tagLoader, bool metadata)
            : base(performanceCounterGroup, sendBusiness, tagLoader, metadata)
        {
            _metaDataBusiness = new MetaDataBusiness();
        }

        public override async Task<int> CollectRegisterCounterValuesAsync()
        {
            lock (_syncRoot)
            {
                try
                {
                    var swMain = new StopwatchHighPrecision();
                    var timeInfo = new Dictionary<string, long>();

                    double elapseOffsetSeconds = 0;
                    if (_timestamp == null)
                    {
                        _sw = new StopwatchHighPrecision();
                        _timestamp = DateTime.UtcNow;
                        var nw = Floor(_timestamp.Value, new TimeSpan(0, 0, 0, 1));
                        _timestamp = nw;
                    }
                    else
                    {
                        var elapsedTotal = _sw.ElapsedTotal;

                        elapseOffsetSeconds = new TimeSpan(elapsedTotal).TotalSeconds - SecondsInterval * _counter;

                        if (_missCounter >= 6)
                        {
                            //Reset everything and start over.
                            OnCollectRegisterCounterValuesEvent(new CollectRegisterCounterValuesEventArgs(Name, string.Format("Missed {0} cycles. Resetting and starting over.", _missCounter), OutputLevel.Warning));

                            _timestamp = null;
                            _counter = 0;
                            _missCounter = 0;
                            return -4;
                        }

                        if (elapseOffsetSeconds > SecondsInterval)
                        {
                            _missCounter++;
                            _counter = _counter + 1 + (int)(elapseOffsetSeconds / SecondsInterval);
                            OnCollectRegisterCounterValuesEvent(new CollectRegisterCounterValuesEventArgs(Name, string.Format("Dropping {0} steps.", (int)elapseOffsetSeconds), OutputLevel.Warning));
                            return -2;
                        }

                        if (elapseOffsetSeconds < SecondsInterval * -1)
                        {
                            _missCounter++;
                            OnCollectRegisterCounterValuesEvent(new CollectRegisterCounterValuesEventArgs(Name, string.Format("Jumping 1 step. ({0} = new TimeSpan({1}).TotalSeconds - {2} * {3})", (int)elapseOffsetSeconds, elapsedTotal, SecondsInterval, _counter), OutputLevel.Warning));
                            return -3;
                        }

                        _missCounter = 0;

                        //Adjust interval
                        var next = 1000 * (SecondsInterval - elapseOffsetSeconds);
                        if (next > 0)
                        {
                            SetTimerInterval(next);
                        }
                    }

                    var timestamp = _timestamp.Value.AddSeconds(SecondsInterval * _counter);
                    _counter++;

                    var precision = TimeUnit.Seconds;
                    timeInfo.Add(TimerConstants.Synchronize, swMain.ElapsedSegment);

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
                        Enqueue(_metaDataBusiness.GetCollectorPoint(EngineName, Name, points.Length, timeInfo, elapseOffsetSeconds).ToArray());
                    }

                    OnCollectRegisterCounterValuesEvent(new CollectRegisterCounterValuesEventArgs(Name, points.Count(), timeInfo, elapseOffsetSeconds, OutputLevel.Default));

                    //TODO: Release mutex

                    //TOOD: Send metadata about the read to influx, (this should be configurable)

                    return points.Length;
                }
                catch (Exception exception)
                {
                    OnCollectRegisterCounterValuesEvent(new CollectRegisterCounterValuesEventArgs(Name, exception));
                    return -1;
                }
            }
        }
    }
}