using System.Text;
using System.Xml;

namespace Tharga.InfluxCapacitor.Collector
{
    public static class XmlDocumentExtensions
    {
        public static string ToFormattedString(this XmlDocument document)
        {
            var sb = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = "\r\n", NewLineHandling = NewLineHandling.Replace };

            using (var writer = XmlWriter.Create(sb, settings))
            {
                document.Save(writer);
            }

            var contents = sb.ToString();
            return contents;
        }
    }
}