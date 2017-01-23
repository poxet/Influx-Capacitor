using System;
using System.Text.RegularExpressions;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class CounterInstanceFilter : ICounterInstanceFilter
    {
        private readonly string _pattern;
        private readonly string _replacement;

        public CounterInstanceFilter(string pattern, string replacement)
        {
            if (pattern == null) throw new ArgumentNullException("pattern");

            _pattern = pattern;
            _replacement = replacement;
        }

        public string Execute(string input)
        {
            if (input == null)
            {
                return null;
            }

            if (_replacement != null)
            {
                return Regex.Replace(input, _pattern, _replacement);
            }

            if (Regex.IsMatch(input, _pattern))
            {
                return input;
            }

            return null;
        }
    }
}