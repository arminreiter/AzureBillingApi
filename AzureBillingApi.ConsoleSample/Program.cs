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
        const char SEP = ';'; // seperator for CSV export

        static void Main(string[] args)
        {
            Client c = new Client(
                "[TENANT].onmicrosoft.com",
                "[CLIENTID]",
                "[CLIENTSECRET]",
                "[SUBSCRIPTIONID]",
                "http://[REDIRECTURL]");

            var resourceData = c.GetResourceCosts("MS-AZR-0003p", "EUR", "de-AT", "AT",
                new DateTime(2017, 3, 1, 0, 0, 0), new DateTime(2017, 3, 6, 23, 0, 0), AggregationGranularity.Daily, true);
            
            Console.WriteLine(resourceData.TotalCosts + " " + resourceData.RateCardData.Currency);
            PrintMeters(resourceData); // Print costs per meter
            //PrintResources(resourceData); // Print costs per resourcename per meter

            string csvPath = EnterCsvPath(); // Export to CSV?
            if (string.IsNullOrEmpty(csvPath))
                return;
            
            // Create CSV with ratecard data
            var rccsv = CreateCsv(resourceData.Costs.Select(x => x.RateCardMeter).ToList());
            System.IO.File.WriteAllText(GetCsvPath(csvPath, "ratecard.csv"), rccsv, Encoding.UTF8);
            Console.WriteLine("created ratecard.csv");

            // Create CSV with usage data
            var uscsv = CreateCsv(resourceData.Costs.Select(x => x.UsageValue).ToList());
            System.IO.File.WriteAllText(GetCsvPath(csvPath, "usage.csv"), uscsv, Encoding.UTF8);
            Console.WriteLine("created usage.csv");

            // Create CSV with costs
            var csv = CreateCsv(resourceData);
            System.IO.File.WriteAllText(GetCsvPath(csvPath, "costs.csv"), csv, Encoding.UTF8);
            Console.WriteLine("created costs.csv");

            System.Diagnostics.Process.Start(csvPath); // start explorer
        }

        /// <summary>
        /// Creates a csv file containing the costs per meter (ratecard data)
        /// </summary>
        /// <param name="meters">ratecard data</param>
        /// <returns></returns>
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

        /// <summary>
        /// creates a csv file with the usage including the costs
        /// </summary>
        /// <param name="data">data from the resource cost data</param>
        /// <returns>csv file as string</returns>
        public static string CreateCsv(ResourceCostData data)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Resource{SEP}Meter Name{SEP}Usage{SEP}Billable{SEP}Costs{SEP}");

            var resourceNames = data.GetResourceNames();

            object sblock = new object();
            System.Threading.Tasks.Parallel.ForEach(resourceNames, resource =>
            {
                var resourceValues = data.Costs.GetCostsByResourceName(resource);
                var meterIds = resourceValues.GetUsedMeterIds();

                System.Threading.Tasks.Parallel.ForEach(meterIds, x =>
                {
                    var currates = resourceValues.GetCostsByMeterId(x);
                    string metername = data.GetMeterById(x).MeterName;
                    var curcosts = currates.Sum(y => y.CalculatedCosts);
                    var usage = currates.Sum(y => y.UsageValue.Properties.Quantity);

                    var billable = currates.Sum(y => y.BillableUnits);
                    lock (sblock)
                    {
                        sb.AppendLine($"{resource}{SEP}{metername}{SEP}{usage.Print()}{SEP}{billable.Print()}{SEP}{curcosts.Print()}");
                    }
                });
            });
            
            return sb.ToString();
        }

        /// <summary>
        /// Print usage and costs to the console, aggregated by meters
        /// </summary>
        /// <param name="resourceCosts">the resource cost data</param>
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

        /// <summary>
        /// Print usage and costs to the console, aggregated by resource names and meters
        /// </summary>
        /// <param name="data"></param>
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

        /// <summary>
        /// Reads csv folder path from user input
        /// </summary>
        /// <returns></returns>
        private static string EnterCsvPath()
        {
            Console.WriteLine("Please enter a path (folder) for the CSV files, leave empty to skip generation of CSV files:");
            string csvPath = string.Empty;
            do
            {
                if (!string.IsNullOrEmpty(csvPath))
                    Console.WriteLine(csvPath + " does not exist, try again: ");
                csvPath = Console.ReadLine();
            }
            while (!System.IO.Directory.Exists(csvPath) && !String.IsNullOrEmpty(csvPath));

            return csvPath;
        }

        /// <summary>
        /// Combines folder and filename and adds the current date to the filename
        /// </summary>
        /// <param name="folder">path of the folder where the file will be stored</param>
        /// <param name="filename">name of the file</param>
        /// <returns>folder + date + filename - e.g. c:\data\20170228_myfile.csv</returns>
        private static string GetCsvPath(string folder, string filename)
        {
            return System.IO.Path.Combine(folder, DateTime.Now.ToString("yyyyMMdd") + "_" + filename);
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
