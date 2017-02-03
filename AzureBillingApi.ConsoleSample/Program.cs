using System;
using System.Linq;

namespace CodeHollow.AzureBillingApi.ConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Client c = new Client(
                "xxx.onmicrosoft.com", 
                "[CLIENTID]", 
                "[CLIENTSECRET]",
                "[SUBSCRIPTIONID]", 
                "http://localhost/billingapi");

            /*
            var usageData = c.GetUsageData(new DateTime(2017, 1, 14, 0, 0, 0), DateTime.Now, Usage.AggregationGranularity.Daily, true);

            Console.WriteLine("Usage data received: ");
            Console.WriteLine(usageData.Value.Count);

            var ratecardData = c.GetRateCardData("MS-AZR-0003p", "EUR", "de-AT", "AT");

            Combine(usageData, ratecardData);

            Console.WriteLine("RateCard data received: ");
            Console.WriteLine(ratecardData.Meters.Count);
            */
            Console.WriteLine("Combined: ");

            var combined = c.GetResourceCosts("MS-AZR-0003p", "EUR", "de-AT", "AT", new DateTime(2017, 1, 14, 0, 0, 0), DateTime.Now, Usage.AggregationGranularity.Daily, true);

            var costs = from x in combined select x.Costs;
            var totalCosts = costs.Sum();

            Console.WriteLine(totalCosts + " EUR");

            var nonempty = from x in combined where x.Costs > 0.0 select x;
            var meterIds = (from x in nonempty select x.RateCardMeter.MeterId).Distinct();

            foreach (var x in meterIds)
            {
                var currates = (from y in nonempty where y.RateCardMeter.MeterId.Equals(x) select y);

                var curcosts = currates.Sum(y => y.Costs);
                var curcostsr = Math.Round(curcosts, 8);

                Console.WriteLine(currates.First().UsageValue.Properties.MeterName.PadRight(72) + " : " + curcosts.ToString("0.###############"));

            }

            Console.Read();

        }
        
    }
}
