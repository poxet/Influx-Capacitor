using System;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Net.Enums;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Entities;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor
{
    public class Measure : IMeasure
    {
        private readonly IQueue _queue;

        public Measure(IQueue queue)
        {
            _queue = queue;
        }

        public T Execute<T>(Func<IMeasurement, T> action)
        {
            return Execute(GetName(action.Method.Name), action);
        }

        public T Execute<T>(Func<T> action)
        {
            return Execute(GetName(action.Method.Name), action);
        }

        public void Execute(Action action)
        {
            Execute(GetName(action.Method.Name), action);
        }

        public void Execute(Action<IMeasurement> action)
        {
            Execute(GetName(action.Method.Name), action);
        }

        public async Task<T> ExecuteAsync<T>(Func<IMeasurement, T> action)
        {
            return await ExecuteAsync(GetName(action.Method.Name), action);
        }

        public async Task<T> ExecuteAsync<T>(Func<T> action)
        {
            return await ExecuteAsync(GetName(action.Method.Name), action);
        }

        public async Task ExecuteAsync(Action action)
        {
            await ExecuteAsync(GetName(action.Method.Name), action);
        }

        public async Task ExecuteAsync(Action<IMeasurement> action)
        {
            await ExecuteAsync(GetName(action.Method.Name), action);
        }

        public T Execute<T>(string measurement, Func<IMeasurement, T> action)
        {
            var m = new Measurement();
            return DoExecute(measurement, m, () => action(m));
        }

        public T Execute<T>(string measurement, Func<T> action)
        {
            return DoExecute(measurement, new Measurement(), action);
        }

        public void Execute(string measurement, Action action)
        {
            DoExecute(measurement, new Measurement(), () =>
            {
                action();
                return true;
            });
        }

        public void Execute(string measurement, Action<IMeasurement> action)
        {
            var m = new Measurement();
            DoExecute(measurement, m, () =>
            {
                action(m);
                return true;
            });
        }

        public async Task<T> ExecuteAsync<T>(string measurement, Func<IMeasurement, T> action)
        {
            return await Task.Run(() =>
            {
                var m = new Measurement();
                return DoExecute(measurement, m, () => action(m));
            });
        }

        public async Task<T> ExecuteAsync<T>(string measurement, Func<T> action)
        {
            return await Task.Run(() => DoExecute(measurement, new Measurement(), action));
        }

        public async Task ExecuteAsync(string measurement, Action action)
        {
            await Task.Run(() =>
            {
                DoExecute(measurement, new Measurement(), () =>
                {
                    action();
                    return true;
                });
            });
        }

        public async Task ExecuteAsync(string measurement, Action<IMeasurement> action)
        {
            await Task.Run(() =>
            {
                var m = new Measurement();
                DoExecute(measurement, m, () =>
                {
                    action(m);
                    return true;
                });
            });
        }

        private string GetName(string name)
        {
            var p1 = name.IndexOf("<", StringComparison.Ordinal);
            var p2 = name.IndexOf(">", p1, StringComparison.Ordinal);
            var result = name.Substring(p1 + 1, p2 - p1 - 1);
            return result;
        }

        private T DoExecute<T>(string measurement, Measurement m, Func<T> action)
        {
            if (string.IsNullOrEmpty(measurement))
                measurement = "Unknown";

            var point = new Point
            {
                Measurement = measurement,
                Precision = TimeUnit.Milliseconds,
                Timestamp = DateTime.UtcNow,
            };

            m.Stopwatch.Reset();
            m.Stopwatch.Start();

            try
            {
                var response = action();
                m.Stopwatch.Stop();
                m.AddTag("isSuccess", true);
                Finalize(m, point);
                return response;
            }
            catch (Exception exp)
            {
                m.Stopwatch.Stop();
                m.AddTag("isSuccess", false);
                m.AddTag("exception", exp.Message);
                Finalize(m, point);
                throw;
            }
        }

        private void Finalize(IMeasurement m, Point point)
        {            
            m.AddField("elapsed", m.GetElapsed().TotalMilliseconds);
            m.AddTag("isCheckpoint", false);

            point.Fields = m.Fields;
            point.Tags = m.Tags;
            _queue.Enqueue(point);

            if (m.Checkpoints.Any())
            {
                //NOTE: Prepare all checkpoints
                var prev = TimeSpan.Zero;
                var index = 0;
                foreach (var checkpoint in m.Checkpoints)
                {
                    var pointFields = point.Fields.Where(x => x.Key != "value").ToDictionary(x => x.Key, x => x.Value);
                    var pointTags = point.Tags.Where(x => x.Key != "checkpoint" && x.Key != "isCheckpoint" && x.Key != "index").ToDictionary(x => x.Key, x => x.Value);
                    pointFields.Add("value", (checkpoint.Value - prev).TotalMilliseconds);
                    pointTags.Add("isCheckpoint", true);
                    pointTags.Add("checkpoint", checkpoint.Key);
                    pointTags.Add("index", index++);

                    var check = new Point
                    {
                        Measurement = point.Measurement,
                        Fields = pointFields,
                        Tags = pointTags,
                        Timestamp = point.Timestamp,
                        Precision = point.Precision,
                    };
                    _queue.Enqueue(check);

                    prev = checkpoint.Value;
                }

                //NOTE: Prepare the end point checkpoint
                {
                    var pointFields = point.Fields.Where(x => x.Key != "value").ToDictionary(x => x.Key, x => x.Value);
                    var pointTags = point.Tags.Where(x => x.Key != "elapsed" && x.Key != "isCheckpoint").ToDictionary(x => x.Key, x => x.Value);
                    pointFields.Add("value", (m.GetElapsed() - prev).TotalMilliseconds);
                    pointTags.Add("isCheckpoint", true);
                    pointTags.Add("checkpoint", "End");
                    pointTags.Add("index", index);

                    var check = new Point
                    {
                        Measurement = point.Measurement,
                        Fields = pointFields,
                        Tags = pointTags,
                        Timestamp = point.Timestamp,
                        Precision = point.Precision,
                    };
                    _queue.Enqueue(check);
                }
            }
        }
    }
}