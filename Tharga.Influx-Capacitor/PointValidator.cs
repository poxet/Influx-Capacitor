using System.Collections.Generic;
using System.Linq;
using InfluxDB.Net.Models;

namespace Tharga.InfluxCapacitor
{
    public class PointValidator
    {
        public IEnumerable<string> Validate(Point point)
        {
            if (string.IsNullOrEmpty(point.Measurement)) yield return "There is no name for measurement.";

            if (!point.Fields.Any()) yield return $"There are no fields for measurement {point.Measurement ?? "n/a"}.";
            foreach(var missing in point.Fields.Where(x => (x.Value?.ToString() ?? "") == ""))
            {
                yield return $"Value missing for field {missing.Key} for measurement {point.Measurement ?? "n/a"}.";
            }

            foreach (var missing in point.Tags.Where(x => (x.Value?.ToString() ?? "") == ""))
            {
                yield return $"Value missing for tag {missing.Key} for measurement {point.Measurement ?? "n/a"}.";
            }
        }

        public IEnumerable<string> Validate(IEnumerable<Point> points)
        {
            return points.SelectMany(Validate);
        }

        public IEnumerable<Point> Clean(IEnumerable<Point> points)
        {
            foreach (var point in points)
            {
                if (!Validate(point).Any())
                {
                    yield return point;
                }
            }
        }
    }
}