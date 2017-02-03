using CodeHollow.AzureBillingApi.RateCard;
using CodeHollow.AzureBillingApi.Usage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            // How to get ratecard and usage data separated and combine it:
            /* 
            var usageData = c.GetUsageData(new DateTime(2017, 1, 14, 0, 0, 0), DateTime.Now, Usage.AggregationGranularity.Daily, true);
            var ratecardData = c.GetRateCardData("MS-AZR-0003p", "EUR", "de-AT", "AT");
            var combined = Client.Combine(usageData, ratecardData);
            */

            Console.WriteLine("Resource costs: ");

            var resourceCosts = c.GetResourceCosts("MS-AZR-0003p", "EUR", "en-US", "AT", new DateTime(2017, 1, 14, 0, 0, 0), DateTime.Now, AggregationGranularity.Daily, true);
            
            var costs = from x in resourceCosts select x.Costs;
            var totalCosts = costs.Sum();

            Console.WriteLine(totalCosts + " EUR");

            
            var resourceNames = (from x in resourceCosts select x.UsageValue.ResourceName).Distinct();

            foreach (var resource in resourceNames)
            {
                var resourceValues = resourceCosts.Where(x => x.UsageValue.ResourceName.Equals(resource));
                var meterIds = (from x in resourceValues select x.RateCardMeter.MeterId).Distinct();

                foreach (var x in meterIds)
                {
                    var currates = (from y in resourceValues where y.RateCardMeter.MeterId.Equals(x) select y);
                    var curcosts = currates.Sum(y => y.Costs);

                    var curusagevalue = currates.First().UsageValue;
                    var resourceName = curusagevalue.Properties.InstanceData.MicrosoftResources.ResourceName;

                    Console.WriteLine(resourceName.PadRight(20) + ": " + currates.First().UsageValue.Properties.MeterName.PadRight(72) + " : " + curcosts.ToString("0.################"));
                }
            }

            // Create CSV:
            //var rccsv = CreateCsv(combined.Select(x => x.RateCardMeter).ToList());
            //var uscsv = CreateCsv(combined.Select(x => x.UsageValue).ToList());
            //System.IO.File.WriteAllText("c:\\data\\rc.csv", rccsv);
            //System.IO.File.WriteAllText("c:\\data\\us.csv", uscsv);

            Console.WriteLine("Press key to exit!");
            Console.ReadKey();
        }

        public static string CreateCsv(List<Meter> meters)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("MeterId;MeterName;MeterCategory;MeterSubCategory;Unit;MeterTags;MeterRegion;MeterRates;EffectiveDate;IncludedQuantity;MeterStatus");

            meters.ForEach(x =>
            {
                string meterRates = string.Join(",", x.MeterRates.Select(y => " [ " + y.Key.ToString() + " : " + y.Value.ToString() + " ]"));
                string meterTags = string.Join(",", x.MeterTags);
                sb.AppendLine($"{x.MeterId};{x.MeterName};{x.MeterCategory};{x.MeterSubCategory};{x.Unit};\"{meterTags}\";{x.MeterRegion};\"{meterRates}\";{x.EffectiveDate};{x.IncludedQuantity};{x.MeterStatus}");
            });

            return sb.ToString();
        }

        /// <summary>
        /// creates a csv file from the usage data
        /// </summary>
        /// <param name="data">data from the usage api</param>
        /// <returns>csv file as string</returns>
        public static string CreateCsv(List<UsageValue> data)
        {
            StringBuilder sb = new StringBuilder();

            string[] columns = new string[]
            {
                "id", "name", "type",
                "properties/subscriptionId", "properties/usageStartTime", "properties/usageEndTime",
                "properties/meterName", "properties/meterCategory", "properties/unit",
                "properties/instanceData", "properties/meterId", "properties/MeterRegion",
                "properties/quantity", "properties/infoFields"
            };

            string header = string.Join(";", columns);
            sb.AppendLine(header);

            data.ForEach(x =>
            {
                string[] values = new string[]
                {
                    x.Id, x.Name, x.Type,
                    x.Properties.SubscriptionId, x.Properties.UsageStartTime, x.Properties.UsageEndTime,
                    x.Properties.MeterName, x.Properties.MeterCategory,x.Properties.Unit,
                    x.Properties.InstanceDataRaw, x.Properties.MeterId, x.Properties.MeterRegion,
                    x.Properties.Quantity.ToString(), JsonConvert.SerializeObject(x.Properties.InfoFields)
                };

                sb.AppendLine(string.Join(";", values));
            });

            return sb.ToString();
        }
    }
}
