using System;
using System.Collections.Generic;

namespace CodeHollow.AzureBillingApi.Usage
{
    /// <summary>
    /// Microsoft.Resources value of the usage aggregate.
    /// </summary>
    [Serializable]
    public class MicrosoftResourcesDataType
    {
        /// <summary>
        /// Fully qualified resource ID, which includes the resource groups and the instance name.
        /// </summary>
        public string ResourceUri { get; set; }

        /// <summary>
        /// Resource tags specified by the user.
        /// </summary>
        public IDictionary<string, string> Tags { get; set; }

        /// <summary>
        /// More details about the resource being consumed. For example, OS version, Image Type.
        /// </summary>
        public IDictionary<string, string> AdditionalInfo { get; set; }

        /// <summary>
        /// The region in which this service run.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Unique namespace used to identify the resource for Azure Marketplace 3rd party usage.
        /// </summary>
        public string PartNumber { get; set; }

        /// <summary>
        /// Unique ID that represents the 3rd party order identifier. Presence of an orderNumber states that this usage record was incurred on a resource owned by a 3rd party and not Microsoft.
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        /// The name of the resource parsed out of the ResourceUri.
        /// </summary>
        public string ResourceName
        {
            get
            {
                return ResourceUri.Substring(ResourceUri.LastIndexOf('/') + 1);
            }
        }
    }
}
