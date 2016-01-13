using System;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Entities
{
    public class SendEventArgs : EventArgs
    {
        private readonly ISenderConfiguration _senderConfiguration;
        private readonly string _message;
        //private readonly OutputLevel _outputLevel;
        private readonly Exception _exception;

        public SendEventArgs(ISenderConfiguration senderConfiguration, Exception exception)
        {
            _senderConfiguration = senderConfiguration;
            _exception = exception;
            //_outputLevel = OutputLevel.Error;
        }

        public SendEventArgs(ISenderConfiguration senderConfiguration, string message, int count) //, OutputLevel outputLevel)
        {
            _senderConfiguration = senderConfiguration;
            _message = message;
            //_outputLevel = outputLevel;
        }

        //public string Message { get { return "Database " + _databaseConfig.Url + "/" + _databaseConfig.Name + ". " + (_exception != null ? _exception.Message : _message); } }
        //public OutputLevel OutputLevel { get { return _outputLevel; } }
    }
}