using InfluxDB.Net.Contracts;
using InfluxDB.Net.Models;

namespace Tharga.InfluxCapacitor.Agents
{
    public class NullFormatter : IFormatter
    {
        public string GetLineTemplate()
        {
            return string.Empty;
        }

        public string PointToString(Point point)
        {
            return string.Empty;
        }

        public Serie PointToSerie(Point point)
        {
            return new Serie();
        }
    }
}