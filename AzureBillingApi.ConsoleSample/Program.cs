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
                "mytenant.onmicrosoft.com",
                "[CLIENTID]",
                "[CLIENTSECRET]",
                "[SUBSCRIPTIONID]",
                "http://[REDIRECTURL]");

            // How to get ratecard and usage data separated and combine it:
            /*
            var usageData = c.GetUsageData(new DateTime(2017, 1, 14, 0, 0, 0), DateTime.Now, Usage.AggregationGranularity.Daily, true);
            var ratecardData = c.GetRateCardData("MS-AZR-0003p", "EUR", "de-AT", "AT");
            var combined = Client.Combine(ratecardData, usageData);
            */

            Console.WriteLine("Resource costs: ");

            var resourceCosts = c.GetResourceCosts("MS-AZR-0003p", "USD", "en-US", "US",
                new DateTime(2016, 11, 14, 0, 0, 0), new DateTime(2016, 12, 13, 23, 0, 0), AggregationGranularity.Daily, true);

            var costs = from x in resourceCosts.Costs select x.CalculatedCosts;
            var totalCosts = costs.Sum();

            Console.WriteLine(totalCosts + " " + resourceCosts.RateCardData.Currency);
            PrintMeters(resourceCosts.Costs);

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

        private static void PrintMeters(List<ResourceCosts> resourceCosts)
        {
            var meterIds = (from x in resourceCosts select x.RateCardMeter.MeterId).Distinct();

            foreach (var x in meterIds)
            {
                var currates = (from y in resourceCosts where y.RateCardMeter.MeterId.Equals(x) select y);
                string metername = currates.First().UsageValue.Properties.MeterName;
                var curcosts = currates.Sum(y => y.CalculatedCosts);
                var billable = currates.Sum(y => y.BillableUnits);
                var usage = currates.Sum(y => y.UsageValue.Properties.Quantity);

                var curusagevalue = currates.First().UsageValue;

                Console.WriteLine($"{metername.PadRight(72)} : {usage.ToString("0.################")} ({billable.ToString("0.################")}) - {curcosts.ToString("0.################")}");
            }
        }

        private static void PrintResources(List<ResourceCosts> resourceCosts)
        {
            var resourceNames = (from x in resourceCosts select x.UsageValue.ResourceName).Distinct();

            foreach (var resource in resourceNames)
            {
                var resourceValues = resourceCosts.Where(x => x.UsageValue.ResourceName.Equals(resource));
                var meterIds = (from x in resourceValues select x.RateCardMeter.MeterId).Distinct();

                foreach (var x in meterIds)
                {
                    var currates = (from y in resourceValues where y.RateCardMeter.MeterId.Equals(x) select y);
                    string metername = currates.First().UsageValue.Properties.MeterName;
                    var curcosts = currates.Sum(y => y.CalculatedCosts);
                    var usage = currates.Sum(y => y.UsageValue.Properties.Quantity);

                    var curusagevalue = currates.First().UsageValue;

                    Console.WriteLine(resource.PadRight(20) + ": " + metername.PadRight(72) + " : " + usage.ToString("0.################") + " - " + curcosts.ToString("0.################"));
                }
            }
        }
    }
}
