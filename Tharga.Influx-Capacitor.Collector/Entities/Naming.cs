namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class Naming
    {
        public Naming(string name, string alias = null)
        {
            Name = name;
            Alias = alias == "" ? null : alias;
        }

        public string Name { get; private set; }
        public string Alias { get; private set; }

        public override string ToString()
        {
            return Alias ?? Name;
        }
    }
}