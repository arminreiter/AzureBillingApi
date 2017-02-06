using System;
using System.Collections.Generic;
using System.Linq;
using CodeHollow.AzureBillingApi.RateCard;

namespace CodeHollow.AzureBillingApi
{
    /// <summary>
    /// Resource cost data contains the list of costs and the ratecard data.
    /// </summary>
    [Serializable]
    public class ResourceCostData
    {
        /// <summary>
        /// The costs which is the combination of usage with ratecard and the cost calculation.
        /// </summary>
        public List<ResourceCosts> Costs { get; set; }

        /// <summary>
        /// The whole ratecard data set.
        /// </summary>
        public RateCardData RateCardData { get; set; }

        /// <summary>
        /// Returns the sum of all calculated costs (in <see cref=" Costs"/>)
        /// </summary>
        public double TotalCosts
        {
            get
            {
                return Costs.Sum(x => x.CalculatedCosts);
            }
        }
       
    }
}
