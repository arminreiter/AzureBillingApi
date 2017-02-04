using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CodeHollow.AzureBillingApi.Usage
{
    /// <summary>
    /// The usage data that comes from the usage api.
    /// </summary>
    [Serializable]
    public class UsageData
    {
        /// <summary>
        /// The usage values.
        /// </summary>
        [JsonProperty("value")]
        public List<UsageValue> Values { get; set; }

        /// <summary>
        /// returns the full url, including query parameters and continuation token
        /// for the next page (usage api returns only 1000 items)
        /// </summary>
        public string NextLink { get; set; }
    }
}
