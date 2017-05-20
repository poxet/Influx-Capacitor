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
        Task ExecuteAsync<T>(Func<Task> action);
        Task ExecuteAsync<T>(Func<IMeasurement, Task> action);
        Task ExecuteAsync<T>(string measurement, Func<IMeasurement, Task> action);
        Task<T> ExecuteAsync<T>(Func<Task<T>> action);
        Task<T> ExecuteAsync<T>(Func<IMeasurement, Task<T>> action);
        Task<T> ExecuteAsync<T>(string measurement, Func<IMeasurement, Task<T>> action);
    }
}