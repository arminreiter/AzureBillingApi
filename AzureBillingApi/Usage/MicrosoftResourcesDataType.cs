using System.Collections.Generic;

namespace CodeHollow.AzureBillingApi.Usage
{
    public class MicrosoftResourcesDataType
    {
        public string ResourceUri { get; set; }
        public IDictionary<string, string> Tags { get; set; }
        public IDictionary<string, string> AdditionalInfo { get; set; }
        public string Location { get; set; }
        public string PartNumber { get; set; }
        public string OrderNumber { get; set; }

        public string ResourceName
        {
            get
            {
                return ResourceUri.Substring(ResourceUri.LastIndexOf('/') + 1);
            }
        }
    }
}
