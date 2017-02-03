using System.Collections.Generic;

namespace CodeHollow.AzureBillingApi.Usage
{
    public class UsageData
    {
        public List<UsageValue> Value { get; set; }

        /// <summary>
        /// returns the full url, including query parameters and continuation token
        /// for the next page (usage api returns only 1000 items)
        /// </summary>
        public string NextLink { get; set; }
    }
}
