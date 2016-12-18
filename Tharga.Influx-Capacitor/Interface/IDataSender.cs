//TODO: Clean
//using System;
//using InfluxDB.Net.Models;
//using Tharga.InfluxCapacitor.Entities;

//namespace Tharga.InfluxCapacitor.Interface
//{
//    public interface IDataSender
//    {
//        event EventHandler<SendCompleteEventArgs> SendCompleteEvent;
//        SendResponse Send();
//        void Enqueue(Point[] points);
//        string TargetServer { get; }
//        string TargetDatabase { get; }
//        int QueueCount { get; }
//    }
//}