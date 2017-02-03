namespace CodeHollow.AzureBillingApi
{
    /// <summary>
    /// Costs of the resources - combination of usage data and ratecard data with calculated costs.
    /// </summary>
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
        public double Costs { get; set; }
    }
}
