using CodeHollow.AzureBillingApi.RateCard;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Get all meter ids that are used. Returns the meter ids of all used meters. All available meters are part of <see cref="RateCardData"/>
        /// </summary>
        /// <param name="costs">list of the costs</param>
        /// <returns>list of meter ids</returns>
        public static IEnumerable<string> GetUsedMeterIds(this IEnumerable<ResourceCosts> costs)
        {
            return costs.Select(x => x.RateCardMeter.MeterId).Distinct();
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
            return costs.Select(x => x.RateCardMeter).Where(x => x.MeterId.Equals(meterId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
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
        /// Returns the meter ids of all used meters. All available meters are part of <see cref="RateCardData"/>
        /// </summary>
        /// <returns>Meter Ids</returns>
        public static IEnumerable<string> GetUsedMeterIds(this ResourceCostData data)
        {
            return data.Costs.GetUsedMeterIds();
        }

        /// <summary>
        /// Returns the resource names of all used resources.
        /// </summary>
        /// <returns>Resource names</returns>
        public static IEnumerable<string> GetResourceNames(this ResourceCostData data)
        {
            return data.Costs.GetResourceNames();
        }
    }
}
