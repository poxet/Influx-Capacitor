using System;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Entities;

namespace Tharga.InfluxCapacitor.Interface
{
    public interface IMeasure
    {
        void Execute(Action action);
        void Execute(Action<Measurement> action);
        void Execute(string measurement, Action action);
        void Execute(string measurement, Action<Measurement> action);
        T Execute<T>(Func<Measurement, T> action);
        T Execute<T>(Func<T> action);
        T Execute<T>(string measurement, Func<Measurement, T> action);
        T Execute<T>(string measurement, Func<T> action);
        Task ExecuteAsync(Action action);
        Task ExecuteAsync(Action<Measurement> action);
        Task ExecuteAsync(string measurement, Action action);
        Task ExecuteAsync(string measurement, Action<Measurement> action);
        Task<T> ExecuteAsync<T>(Func<Measurement, T> action);
        Task<T> ExecuteAsync<T>(Func<T> action);
        Task<T> ExecuteAsync<T>(string measurement, Func<Measurement, T> action);
        Task<T> ExecuteAsync<T>(string measurement, Func<T> action);
    }
}