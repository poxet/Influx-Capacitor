namespace Tharga.InfluxCapacitor.Collector
{
    internal static class StringExtensions
    {
        public static string Clean(this string data)
        {
            if (data == null) return null;

            data = data.Replace(" ", "");
            data = data.Replace("[", "");
            data = data.Replace("]", "");
            data = data.Replace("{", "");
            data = data.Replace("}", "");
            return data;
        }
    }
}