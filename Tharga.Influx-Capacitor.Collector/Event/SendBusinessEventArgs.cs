//using System;
//using Tharga.InfluxCapacitor.Collector.Interface;
//using Tharga.Toolkit.Console.Command.Base;

//namespace Tharga.InfluxCapacitor.Collector.Event
//{
//    public class SendBusinessEventArgs : EventArgs
//    {
//        private readonly IDatabaseConfig _databaseConfig;
//        private readonly string _message;
//        private readonly OutputLevel _outputLevel;
//        private readonly Exception _exception;

//        public SendBusinessEventArgs(IDatabaseConfig databaseConfig, Exception exception)
//        {
//            _databaseConfig = databaseConfig;
//            _exception = exception;
//            _outputLevel = OutputLevel.Error;
//        }

//        public SendBusinessEventArgs(IDatabaseConfig databaseConfig, string message, int count, OutputLevel outputLevel)
//        {
//            _databaseConfig = databaseConfig;
//            _message = message;
//            _outputLevel = outputLevel;
//        }

//        public string Message { get { return "Database " + _databaseConfig.Url + "/" + _databaseConfig.Name + ". " + (_exception != null ? _exception.Message : _message); } }
//        public OutputLevel OutputLevel { get { return _outputLevel; } }
//    }
//}