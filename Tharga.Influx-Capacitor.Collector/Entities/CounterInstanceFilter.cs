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

        public Naming Execute(Naming input)
        {
            if (input == null)
            {
                return null;
            }

            if (_replacement != null)
            {
                return new Naming(Regex.Replace(input.Name, _pattern, _replacement), input.Alias);
            }

            if (Regex.IsMatch(input.Name, _pattern))
            {
                return input;
            }

            return null;
        }
    }
}