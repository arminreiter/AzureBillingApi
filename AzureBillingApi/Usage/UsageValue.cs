using System;

namespace CodeHollow.AzureBillingApi.Usage
{
    /// <summary>
    /// Usage entry of the usage rest api.
    /// </summary>
    public class UsageValue
    {
        /// <summary>
        /// Unique Id for the usage aggregate.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the usage aggregate.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of the usage aggregate.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The usage properties - contains start and end dates, meter name, id, unit and others.
        /// </summary>
        public UsageProperties Properties { get; set; }

        /// <summary>
        /// The resource name.
        /// </summary>
        public string ResourceName
        {
            get
            {
                return Properties?.InstanceData?.MicrosoftResources?.ResourceName;
            }
        }
    }
}
