using System;
using System.Threading.Tasks;

namespace Tharga.InfluxCapacitor.Interface
{
    public interface IMeasure
    {
        void Execute(Action action);
        void Execute(Action<IMeasurement> action);
        void Execute(string measurement, Action action);
        void Execute(string measurement, Action<IMeasurement> action);
        T Execute<T>(Func<IMeasurement, T> action);
        T Execute<T>(Func<T> action);
        T Execute<T>(string measurement, Func<IMeasurement, T> action);
        T Execute<T>(string measurement, Func<T> action);
        Task ExecuteAsync(Action action);
        Task ExecuteAsync(Action<IMeasurement> action);
        Task ExecuteAsync(string measurement, Action action);
        Task ExecuteAsync(string measurement, Action<IMeasurement> action);
        Task<T> ExecuteAsync<T>(Func<IMeasurement, T> action);
        Task<T> ExecuteAsync<T>(Func<T> action);
        Task<T> ExecuteAsync<T>(string measurement, Func<IMeasurement, T> action);
        Task<T> ExecuteAsync<T>(string measurement, Func<T> action);
    }
}