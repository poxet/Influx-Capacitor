//using System;
//using InfluxDB.Net.Models;
//using Tharga.InfluxCapacitor.Collector.Entities;
//using Tharga.InfluxCapacitor.Collector.Event;

//namespace Tharga.InfluxCapacitor.Collector.Interface
//{
//    public interface IDataSender
//    {
//        event EventHandler<SendBusinessEventArgs> SendBusinessEvent;
//        SendResponse Send();
//        void Enqueue(Point[] points);
//        string TargetServer { get; }
//        string TargetDatabase { get; }
//        int QueueCount { get; }
//    }
//}