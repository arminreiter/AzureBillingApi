using System;

namespace CodeHollow.AzureBillingApi.Usage
{
    /// <summary>
    /// Info fields of the usage aggregate - key value pairs of instance details.
    /// </summary>
    [Serializable]
    public class InfoFields
    {
        /// <summary>
        /// Region of the meterId used for billing purposes.
        /// </summary>
        public string MeteredRegion { get; set; }

        /// <summary>
        /// Metered service - e.g. Storage
        /// </summary>
        public string MeteredService { get; set; }

        /// <summary>
        /// Project - name of the instance e.g. for vms
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// MeteredServiceType
        /// </summary>
        public string MeteredServiceType { get; set; }

        /// <summary>
        /// ServiceInfo
        /// </summary>
        public string ServiceInfo { get; set; }
    }
}
