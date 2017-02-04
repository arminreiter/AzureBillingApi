using System;

namespace CodeHollow.AzureBillingApi
{
    /// <summary>
    /// Costs of the resources - combination of usage data and ratecard data with calculated costs.
    /// </summary>
    [Serializable]
    public class ResourceCosts
    {
        /// <summary>
        /// Usage value.
        /// </summary>
        public Usage.UsageValue UsageValue { get; set; }

        /// <summary>
        /// The ratecard that corresponds to the usage.
        /// </summary>
        public RateCard.Meter RateCardMeter { get; set; }

        /// <summary>
        /// Calculated costs.
        /// </summary>
        public double CalculatedCosts { get; set; }

        /// <summary>
        /// The units which are not for free.
        /// </summary>
        public double BillableUnits { get; set; }
    }
}
