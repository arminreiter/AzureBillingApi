using CodeHollow.AzureBillingApi.RateCard;
using CodeHollow.AzureBillingApi.Usage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CodeHollow.AzureBillingApi
{
    /// <summary>
    /// Contains some extensions for filtering or getting data out of the billing api data
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns all costs that are related to the given resource name
        /// </summary>
        /// <param name="costs">list of all costs</param>
        /// <param name="resourceName">the name of the resource - e.g. myresource-vm</param>
        /// <returns>filtered costs where resource name equals the given resource name</returns>
        public static IEnumerable<ResourceCosts> GetCostsByResourceName(this IEnumerable<ResourceCosts> costs, string resourceName)
        {
            return from x in costs where (x.UsageValue.ResourceName ?? String.Empty).Equals(resourceName) select x;
        }

        /// <summary>
        /// Returns all costs that are related to the given resource name
        /// </summary>
        /// <param name="data">all resource cost data</param>
        /// <param name="resourceName">the name of the resource - e.g. myresource-vm</param>
        /// <returns>filtered costs where resource name equals the given resource name</returns>
        public static IEnumerable<ResourceCosts> GetCostsByResourceName(this ResourceCostData data, string resourceName)
        {
            return GetCostsByResourceName(data.Costs, resourceName);
        }

        /// <summary>
        /// Returns all costs in a dictionary with resourcename as keys
        /// </summary>
        /// <param name="costs">resource cost data</param>
        /// <returns>costs by resource name</returns>
        public static Dictionary<string, IEnumerable<ResourceCosts>> GetCostsByResourceName(this IEnumerable<ResourceCosts> costs)
        {
            var resourceNames = GetResourceNames(costs);
            var result = new Dictionary<string, IEnumerable<ResourceCosts>>();

            foreach (var resource in resourceNames)
            {
                result.Add(resource, costs.GetCostsByResourceName(resource));
            }

            return result;
        }

        /// <summary>
        /// Returns all costs in a dictionary with resourcename as keys
        /// </summary>
        /// <param name="data">resource cost data</param>
        /// <returns>costs by resource name</returns>
        public static Dictionary<string, IEnumerable<ResourceCosts>> GetCostsByResourceName(this ResourceCostData data)
        {
            return GetCostsByResourceName(data.Costs);
        }

        /// <summary>
        /// Get all meter ids that are used. Returns the meter ids of all used meters. All available meters are part of <see cref="RateCardData"/>
        /// </summary>
        /// <param name="costs">list of the costs</param>
        /// <returns>list of meter ids</returns>
        public static IEnumerable<string> GetUsedMeterIds(this IEnumerable<ResourceCosts> costs)
        {
            return costs.Select(x => x.RateCardMeter.MeterId).Distinct();
        }

        /// <summary>
        /// Returns the meter ids of all used meters. All available meters are part of <see cref="RateCardData"/>
        /// </summary>
        /// <param name="data">resource cost data</param>
        /// <returns>Meter Ids</returns>
        public static IEnumerable<string> GetUsedMeterIds(this ResourceCostData data)
        {
            return data.Costs.GetUsedMeterIds();
        }

        /// <summary>
        /// Returns all costs that are related to the given meter id
        /// </summary>
        /// <param name="costs">list of the costs</param>
        /// <param name="meterId">id of the meter</param>
        /// <returns>list of costs filtered by meter id</returns>
        public static IEnumerable<ResourceCosts> GetCostsByMeterId(this IEnumerable<ResourceCosts> costs, string meterId)
        {
            return (from x in costs where x.RateCardMeter.MeterId.Equals(meterId) select x).ToList();
        }

        /// <summary>
        /// Returns all costs by meter name
        /// </summary>
        /// <param name="costs">list of costs</param>
        /// <returns>list of costs by meter name</returns>
        public static Dictionary<string, IEnumerable<ResourceCosts>> GetCostsByMeterName(this IEnumerable<ResourceCosts> costs)
        {
            var result = new Dictionary<string, IEnumerable<ResourceCosts>>();
            var meterIds = costs.GetUsedMeterIds();
            foreach (var meterId in meterIds)
            {
                var meterName = costs.GetMeterById(meterId).MeterName;
                result.Add(meterName, costs.GetCostsByMeterId(meterId));
            }
            return result;
        }

        /// <summary>
        /// Returns all costs by resource name and meter name
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, IEnumerable<ResourceCosts>>> GetCostsByResourceNameAndMeterName(this ResourceCostData data)
        {
            var result = from costsByResourceName in data.GetCostsByResourceName()
                         from costsByMeterName in costsByResourceName.Value.GetCostsByMeterName()
                         select new
                         {
                             ResourceName = costsByResourceName.Key,
                             MeterName = costsByMeterName.Key,
                             Costs = costsByMeterName.Value
                         };
            
            var asdf = result.ToDictionary(
                key => key.ResourceName,
                value => result.ToDictionary( 
                    kkey => value.MeterName, 
                    vvalue => value.Costs));

            return asdf;
        }

        /// <summary>
        /// Returns the meter data by the given meter id.
        /// </summary>
        /// <param name="data">resource cost data</param>
        /// <param name="meterId">id of the meter</param>
        /// <returns>the meter that matches the meterId</returns>
        public static Meter GetMeterById(this ResourceCostData data, string meterId)
        {
            return data.Costs.GetMeterById(meterId);
        }

        /// <summary>
        /// Returns the meter data by the given meter id.
        /// </summary>
        /// <param name="costs">list of the costs</param>
        /// <param name="meterId">id of the meter</param>
        /// <returns>the meter that matches the meterId</returns>
        public static Meter GetMeterById(this IEnumerable<ResourceCosts> costs, string meterId)
        {
            return costs.Select(x => x.RateCardMeter).Where(x => x.MeterId.Equals(meterId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }

        /// <summary>
        /// Returns all used resource names
        /// </summary>
        /// <param name="costs">list of the costs</param>
        /// <returns>list of all resource names</returns>
        public static IEnumerable<string> GetResourceNames(this IEnumerable<ResourceCosts> costs)
        {
            return costs.Select(x => x.UsageValue.ResourceName ?? String.Empty).Distinct();
        }


        /// <summary>
        /// Returns the resource names of all used resources.
        /// </summary>
        /// <param name="data">resource cost data</param>
        /// <returns>Resource names</returns>
        public static IEnumerable<string> GetResourceNames(this ResourceCostData data)
        {
            return data.Costs.GetResourceNames();
        }

        /// <summary>
        /// Returns the names of all used resource groups.
        /// </summary>
        /// <param name="data">resource cost data</param>
        /// <returns>names of the resource groups</returns>
        public static IEnumerable<string> GetResourceGroupNames(this ResourceCostData data)
        {
            var resourceGroups = data.Costs.Select(x => x.UsageValue.GetResourceGroupName() ?? String.Empty);
            return resourceGroups.Distinct();
        }

        /// <summary>
        /// Returns the costs of the given resource group
        /// </summary>
        /// <param name="costs">list of the costs</param>
        /// <param name="resourceGroupName">name of the resource group</param>
        /// <returns>List of costs</returns>
        public static IEnumerable<ResourceCosts> GetCostsByResourceGroup(this IEnumerable<ResourceCosts> costs, string resourceGroupName)
        {
            return from x in costs
                   where (x.UsageValue.GetResourceGroupName()).Equals(resourceGroupName)
                   select x;
        }

        /// <summary>
        /// Returns the costs by resource group
        /// </summary>
        /// <param name="data">resource cost data</param>
        /// <returns>dictionary with resource group name and the related costs</returns>
        public static Dictionary<string, IEnumerable<ResourceCosts>> GetCostsByResourceGroup(this ResourceCostData data)
        {
            var result = from rgn in data.GetResourceGroupNames()
                         select new
                         {
                             ResourceGroupName = rgn,
                             Costs = data.Costs.GetCostsByResourceGroup(rgn)
                         };

            return result.ToDictionary(x => x.ResourceGroupName, y => y.Costs);
        }

        /// <summary>
        /// Returns the name of the resource group by parsing it from the resource uri
        /// </summary>
        /// <param name="data">Instance data</param>
        /// <returns>resource group name</returns>
        public static string GetResourceGroupName(this MicrosoftResourcesDataType data)
        {
            if (String.IsNullOrEmpty(data.ResourceUri))
                return String.Empty;

            Regex rex = new Regex("/subscriptions/(?<id>.*)/resourceGroups/(?<rgname>.*)/providers");

            var matches = rex.Match(data.ResourceUri);
            if (matches.Success && matches.Groups["rgname"].Success)
                return matches.Groups["rgname"].Value;

            return String.Empty;
        }

        /// <summary>
        /// Returns the name of the resource group by parsing it from the resource uri
        /// </summary>
        /// <param name="data">Usage data</param>
        /// <returns>resource group name</returns>
        public static string GetResourceGroupName(this UsageValue data)
        {
            return data?.Properties?.InstanceData?.MicrosoftResources?.GetResourceGroupName() ?? String.Empty;
        }

        /// <summary>
        /// Returns the sum of all CalculatedCosts in costs
        /// </summary>
        /// <param name="costs">list of costs</param>
        /// <returns>sum of calculated costs</returns>
        public static double GetTotalCosts(this IEnumerable<ResourceCosts> costs)
        {
            return costs.Sum(x => x.CalculatedCosts);
        }

        /// <summary>
        /// Returns the sum quantity (resource consumption) in costs
        /// </summary>
        /// <param name="costs">list of costs</param>
        /// <returns>sum of quantity</returns>
        public static double GetTotalUsage(this IEnumerable<ResourceCosts> costs)
        {
            return costs.Sum(x => x.UsageValue.Properties.Quantity);
        }

        /// <summary>
        /// Returns the sum of all billable units in costs
        /// </summary>
        /// <param name="costs">list of costs</param>
        /// <returns>sum of all billable units</returns>
        public static double GetTotalBillable(this IEnumerable<ResourceCosts> costs)
        {
            return costs.Sum(y => y.BillableUnits);
        }
    }
}
