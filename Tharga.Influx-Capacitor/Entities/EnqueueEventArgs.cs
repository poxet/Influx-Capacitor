using System;
using InfluxDB.Net.Models;

namespace Tharga.InfluxCapacitor.Entities
{
    public class EnqueueEventArgs : EventArgs
    {
        public EnqueueEventArgs(Point[] enqueuedPoints, Point[] providedPoints, string[] validationErrors)
        {
            EnqueuedPoints = enqueuedPoints;
            ProvidedPoints = providedPoints;
            ValidationErrors = validationErrors;
        }

        public Point[] EnqueuedPoints { get; }
        public Point[] ProvidedPoints { get; }
        public string[] ValidationErrors { get; }
    }
}