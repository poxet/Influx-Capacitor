using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class CounterCategoryListCommand : ActionCommandBase
    {
        public CounterCategoryListCommand()
            : base("Categories", "Lists all native counter categories (groups) that can be read.")
        {
        }

        public CounterCategoryListCommand(string[] names, string description)
            : base(names, description)
        {
        }

        public override async Task<bool> InvokeAsync(string paramList)
        {
            var counterCategories = PerformanceCounterCategory.GetCategories();

            //Change culture and get counters again
            if (Thread.CurrentThread.CurrentCulture.Name != "en-US")
            {
                var culture = Thread.CurrentThread.CurrentCulture;
                try
                {
                    var counterCategoriesInEnglish = PerformanceCounterCategory.GetCategories();

                    var countersOnlyInEnglish = counterCategoriesInEnglish.Where(x => counterCategories.All(y => y.CategoryName != x.CategoryName)).ToArray();
                    OutputInformation("Counters only in english:");
                    foreach (var category in countersOnlyInEnglish)
                    {
                        OutputInformation("{0}", category.CategoryName);
                    }
                    OutputInformation("There are {0} categories in total.", countersOnlyInEnglish.Length);
                    OutputInformation("");

                    var countersOnlyInCurrent = counterCategories.Where(x => counterCategoriesInEnglish.All(y => y.CategoryName != x.CategoryName)).ToArray();
                    OutputInformation("Counters only in {0}:", culture.Name);
                    foreach (var category in countersOnlyInCurrent)
                    {
                        OutputInformation("{0}", category.CategoryName);
                    }
                    OutputInformation("There are {0} categories in total.", countersOnlyInCurrent.Length);

                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
                }
                finally
                {
                    Thread.CurrentThread.CurrentCulture = culture;
                }
            }
            else
            {
                foreach (var category in counterCategories)
                {
                    OutputInformation("{0}", category.CategoryName);
                }
                OutputInformation("There are {0} categories in total.", counterCategories.Length);
            }

            return true;
        }
    }
}