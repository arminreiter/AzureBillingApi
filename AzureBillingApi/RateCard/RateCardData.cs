using System;
using System.Collections.Generic;

namespace CodeHollow.AzureBillingApi.RateCard
{
    /// <summary>
    /// RateCard data from the azure billing ratecard api.
    /// according to specification: https://msdn.microsoft.com/en-us/library/azure/mt219004.aspx
    /// </summary>
    [Serializable]
    public class RateCardData
    {
        /// <summary>
        /// The offer terms.
        /// </summary>
        public List<object> OfferTerms { get; set; }

        /// <summary>
        /// Meter values.
        /// </summary>
        public List<Meter> Meters { get; set; }

        /// <summary>
        /// The currency in which the rates are provided.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// The culture in which the resource information is localized.
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// All rates are pretax, so this will always be returned as "false".
        /// </summary>
        public bool IsTaxIncluded { get; set; }
    }
}
