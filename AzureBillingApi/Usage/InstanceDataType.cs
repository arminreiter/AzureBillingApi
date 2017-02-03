using Newtonsoft.Json;

namespace CodeHollow.AzureBillingApi.Usage
{
    /// <summary>
    /// Instance data type returned by usage rest api
    /// </summary>
    public class InstanceDataType
    {
        /// <summary>
        /// Microsoft.Resource property
        /// </summary>
        [JsonProperty("Microsoft.Resources")]
        public MicrosoftResourcesDataType MicrosoftResources { get; set; }
    }
}
