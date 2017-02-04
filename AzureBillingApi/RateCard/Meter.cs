using System;
using System.Collections.Generic;

namespace CodeHollow.AzureBillingApi.RateCard
{
    /// <summary>
    /// Meter information, contains region, tags, rates and others
    /// </summary>
    [Serializable]
    public class Meter
    {
        /// <summary>
        /// The unique identifier of the resource.
        /// </summary>
        public string MeterId { get; set; }

        /// <summary>
        /// The name of the meter, within the given meter category
        /// </summary>
        public string MeterName { get; set; }

        /// <summary>
        /// The category of the meter, e.g., "Cloud services", "Networking", etc..
        /// </summary>
        public string MeterCategory { get; set; }

        /// <summary>
        /// The subcategory of the meter, e.g., "A6 Cloud services", "ExpressRoute (IXP)", etc..
        /// </summary>
        public string MeterSubCategory { get; set; }

        /// <summary>
        /// he unit in which the meter consumption is charged, e.g., "Hours", "GB", etc..
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Additional meter data.
        /// </summary>
        public List<string> MeterTags { get; set; }

        /// <summary>
        /// The region in which the Azure service is available.
        /// </summary>
        public string MeterRegion { get; set; }

        /// <summary>
        /// The list of key/value pairs for the meter rates, in the format "key":"value" where key = the meter quantity, and value = the corresponding price.
        /// </summary>
        public Dictionary<double, double> MeterRates { get; set; }

        /// <summary>
        /// Indicates the date from which the meter rate or offer term is effective.
        /// </summary>
        public string EffectiveDate { get; set; }

        /// <summary>
        /// The resource quantity that is included in the offer at no cost. Consumption beyond this quantity will be charged.
        /// </summary>
        public double IncludedQuantity { get; set; }

        /// <summary>
        /// ndicates whether the MeterId is "Active" or "Deprecated".
        /// </summary>
        public string MeterStatus { get; set; }
    }
}
