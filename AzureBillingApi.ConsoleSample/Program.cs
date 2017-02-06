using CodeHollow.AzureBillingApi.RateCard;
using CodeHollow.AzureBillingApi.Usage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CodeHollow.AzureBillingApi.ConsoleSample
{
    class Program
    {
        const char SEP = ';';
        
        static void Main(string[] args)
        {
            Client c = new Client(
                "[TENANT].onmicrosoft.com",
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

            var resourceData = c.GetResourceCostsForPeriod("MS-AZR-0003p", "EUR", "de-AT", "AT",2016,11);

            //var resourceData = c.GetResourceCosts("MS-AZR-0003p", "EUR", "de-AT", "AT",
            //    new DateTime(2016, 12, 14, 0, 0, 0), new DateTime(2017, 1, 1, 23, 0, 0), AggregationGranularity.Daily, true);

            
            Console.WriteLine(resourceData.TotalCosts + " " + resourceData.RateCardData.Currency);
            PrintMeters(resourceData);
            //PrintResources(resourceData);

            // Create CSV:
            //var rccsv = CreateCsv(combined.Select(x => x.RateCardMeter).ToList());
            //System.IO.File.WriteAllText("c:\\data\\rc.csv", rccsv);
            //var uscsv = CreateCsv(combined.Select(x => x.UsageValue).ToList());
            //System.IO.File.WriteAllText("c:\\data\\us.csv", uscsv);
            //var csv = CreateCsv(resourceData);
            //System.IO.File.WriteAllText("c:\\data\\resourcecosts.csv", csv);

            Console.WriteLine("Press key to exit!");
            Console.ReadKey();
        }

        public static string CreateCsv(List<Meter> meters)
        {
            StringBuilder sb = new StringBuilder();

            string[] columns = new string[] {
                "MeterId", "MeterName", "MeterCategory", "MeterSubCategory",
                "Unit", "MeterTags", "MeterRegion", "MeterRates", "EffectiveDate",
                "IncludedQuantity", "MeterStatus"
            };

            sb.AppendLine(string.Join(SEP.ToString(), columns));

            meters.ForEach(x =>
            {
                string meterRates = string.Join(SEP.ToString(), x.MeterRates.Select(y => " [ " + y.Key.ToString() + " : " + y.Value.ToString() + " ]"));
                string meterTags = string.Join(SEP.ToString(), x.MeterTags);
                sb.AppendLine($"{x.MeterId}{SEP}{x.MeterName}{SEP}{x.MeterCategory}{SEP}{x.MeterSubCategory}{SEP}{x.Unit}{SEP}\"{meterTags}\"{SEP}{x.MeterRegion}{SEP}\"{meterRates}\"{SEP}{x.EffectiveDate}{SEP}{x.IncludedQuantity}{SEP}{x.MeterStatus}");
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

            string header = string.Join(SEP.ToString(), columns);
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

                sb.AppendLine(string.Join(SEP.ToString(), values));
            });

            return sb.ToString();
        }

        public static string CreateCsv(ResourceCostData data)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Resource{SEP}Meter Name{SEP}Usage{SEP}Billable{SEP}Costs{SEP}");

            var resourceNames = data.GetResourceNames();

            foreach (var resource in resourceNames)
            {
                var resourceValues = data.Costs.GetCostsByResourceName(resource);
                var meterIds = (from x in resourceValues select x.RateCardMeter.MeterId).Distinct();

                foreach (var x in meterIds)
                {
                    var currates = (from y in resourceValues where y.RateCardMeter.MeterId.Equals(x) select y);
                    string metername = currates.First().UsageValue.Properties.MeterName;
                    var curcosts = currates.Sum(y => y.CalculatedCosts);
                    var usage = currates.Sum(y => y.UsageValue.Properties.Quantity);

                    var billable = currates.Sum(y => y.BillableUnits);
                    var curusagevalue = currates.First().UsageValue;
                    
                    sb.AppendLine($"{resource}{SEP}{metername}{SEP}{usage.Print()}{SEP}{billable.Print()}{SEP}{curcosts.Print()}");
                }
            }
            return sb.ToString();

        }

        private static void PrintMeters(ResourceCostData resourceCosts)
        {
            var meterIds = resourceCosts.GetUsedMeterIds();

            foreach (var x in meterIds)
            {
                var currates = resourceCosts.Costs.GetCostsByMeterId(x);
                string metername = resourceCosts.GetMeterById(x).MeterName;
                var curcosts = currates.Sum(y => y.CalculatedCosts);
                var billable = currates.Sum(y => y.BillableUnits);
                var usage = currates.Sum(y => y.UsageValue.Properties.Quantity);

                var curusagevalue = currates.First().UsageValue;

                Console.WriteLine($"{metername.PadRight(72)} : {usage.Print()} ({billable.Print()}) - {curcosts.Print()}");
            }
        }

        private static void PrintResources(ResourceCostData data)
        {
            var resourceNames = data.GetResourceNames();
            
            foreach (var resource in resourceNames)
            {
                var resourceValues = data.Costs.GetCostsByResourceName(resource);
                var meterIds = resourceValues.GetUsedMeterIds();

                foreach (var x in meterIds)
                {
                    var currates = resourceValues.GetCostsByMeterId(x);
                    string metername = data.GetMeterById(x).MeterName;
                    var curcosts = currates.Sum(y => y.CalculatedCosts);
                    var usage = currates.Sum(y => y.UsageValue.Properties.Quantity);
                    
                    Console.WriteLine(resource.PadRight(20) + ": " + metername.PadRight(72) + " : " + usage.Print() + " - " + curcosts.Print());
                }
            }
        }
    }

    public static class DoubleExtension
    {
        public static string Print(this double data)
        {
            return data.ToString("0.################");
        }
    }
}
