using System;

namespace CodeHollow.AzureBillingApi.Usage
{
    /// <summary>
    /// AggregationGranularity for Usage REST API
    /// </summary>
    [Serializable]
    public enum AggregationGranularity
    {
        /// <summary>
        /// Daily aggregation
        /// </summary>
        Daily,
        /// <summary>
        /// Hourly aggregation
        /// </summary>
        Hourly
    }
}
