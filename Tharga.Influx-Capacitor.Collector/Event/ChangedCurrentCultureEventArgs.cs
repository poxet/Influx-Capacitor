using System;

namespace Tharga.InfluxCapacitor.Collector.Event
{
    public class ChangedCurrentCultureEventArgs : EventArgs
    {
        private readonly string _previousCulture;
        private readonly string _newCulture;

        public ChangedCurrentCultureEventArgs(string previousCulture, string newCulture)
        {
            _previousCulture = previousCulture;
            _newCulture = newCulture;
        }

        public string PreviousCulture
        {
            get { return _previousCulture; }
        }

        public string NewCulture
        {
            get { return _newCulture; }
        }
    }
}