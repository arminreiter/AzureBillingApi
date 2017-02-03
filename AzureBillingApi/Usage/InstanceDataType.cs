using Newtonsoft.Json;

namespace CodeHollow.AzureBillingApi.Usage
{
    public class InstanceDataType
    {
        [JsonProperty("Microsoft.Resources")]
        public MicrosoftResourcesDataType MicrosoftResources { get; set; }
    }
}
