namespace InfluxDB.Net.Collector
{
    internal static class StringExtensions
    {
        public static string Clean(this string data)
        {
            data = data.Replace(" ", "");
            data = data.Replace("[", "");
            data = data.Replace("]", "");
            data = data.Replace("{", "");
            data = data.Replace("}", "");
            return data;
        }
    }
}