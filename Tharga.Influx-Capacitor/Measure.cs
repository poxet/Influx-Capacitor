using System;
using System.Diagnostics;
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

        public T Execute<T>(Func<Measurement, T> action)
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

        public void Execute(Action<Measurement> action)
        {
            Execute(GetName(action.Method.Name), action);
        }

        public async Task<T> ExecuteAsync<T>(Func<Measurement, T> action)
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

        public async Task ExecuteAsync(Action<Measurement> action)
        {
            await ExecuteAsync(GetName(action.Method.Name), action);
        }

        public T Execute<T>(string measurement, Func<Measurement, T> action)
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

        public void Execute(string measurement, Action<Measurement> action)
        {
            var m = new Measurement();
            DoExecute(measurement, m, () =>
            {
                action(m);
                return true;
            });
        }

        public async Task<T> ExecuteAsync<T>(string measurement, Func<Measurement, T> action)
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

        public async Task ExecuteAsync(string measurement, Action<Measurement> action)
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
            var point = new Point
            {
                Measurement = measurement,
                Precision = TimeUnit.Microseconds,
                Timestamp = DateTime.UtcNow,
            };

            var sw = new Stopwatch();
            sw.Start();

            Exception exception = null;
            var response = default(T);
            try
            {
                response = action();
                m.AddTag("IsSuccess", true);
            }
            catch (Exception exp)
            {
                exception = exp;
                m.AddTag("IsSuccess", false);
            }

            sw.Stop();

            m.AddField("Elapsed", sw.Elapsed.TotalMilliseconds);
            m.AddTag("Response", response);

            point.Fields = m.Fields;
            point.Tags = m.Tags;
            _queue.Enqueue(point);

            if (exception != null)
            {
                throw exception;
            }

            return response;
        }
    }
}