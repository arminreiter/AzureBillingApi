using Newtonsoft.Json;
using System;

namespace CodeHollow.AzureBillingApi.Usage
{
    /// <summary>
    /// Usage properties of usage aggregates according to:
    /// https://msdn.microsoft.com/en-us/library/azure/mt219001.aspx
    /// </summary>
    [Serializable]
    public class UsageProperties
    {
        /// <summary>
        /// The subscription identifier for the Azure user.
        /// </summary>
        public string SubscriptionId { get; set; }

        /// <summary>
        /// UTC start time for the usage bucket to which this usage aggregate belongs.
        /// </summary>
        public string UsageStartTime { get; set; }

        /// <summary>
        /// The UsageStartTime as DateTime.
        /// </summary>
        public DateTime UsageStartTimeAsDate {  get { return DateTime.Parse(UsageStartTime); } }

        /// <summary>
        /// UTC end time for the usage bucket to which this usage aggregate belongs.
        /// </summary>
        public string UsageEndTime { get; set; }

        /// <summary>
        /// The UsageEndTime as DateTime.
        /// </summary>
        public DateTime UsageEndTimeAsDateTime { get { return DateTime.Parse(UsageEndTime); } }

        /// <summary>
        /// Unique ID for the resource that was consumed (aka ResourceID).
        /// </summary>
        public string MeterId { get; set; }

        /// <summary>
        /// Friendly name of the resource being consumed.
        /// </summary>
        public string MeterName { get; set; }

        /// <summary>
        /// Category of the consumed resource.
        /// </summary>
        public string MeterCategory { get; set; }

        /// <summary>
        /// Sub-category of the consumed resource.
        /// </summary>
        public string MeterSubCategory { get; set; }

        /// <summary>
        /// Region of the meterId used for billing purposes.
        /// </summary>
        public string MeterRegion { get; set; }

        /// <summary>
        /// Info fields - key value pairs of instance details.
        /// </summary>
        public InfoFields InfoFields { get; set; }

        /// <summary>
        /// InstanceData as json string.
        /// </summary>
        [JsonProperty("instanceData")]
        public string InstanceDataRaw { get; set; }

        /// <summary>
        /// Key-value pairs of instance details - contains resourceUri, tags, location, additionalInfo, partNumber, orderNumber
        /// </summary>
        public InstanceDataType InstanceData
        {
            get
            {
                if (String.IsNullOrEmpty(InstanceDataRaw))
                    return null;

                return JsonConvert.DeserializeObject<InstanceDataType>(InstanceDataRaw.Replace("\\\"", ""));
            }
        }

        /// <summary>
        /// The amount of the resource consumption that occurred in this time frame.
        /// </summary>
        public double Quantity { get; set; }

        /// <summary>
        /// The unit in which the usage for this resource is being counted, e.g.Hours, GB.
        /// </summary>
        public string Unit { get; set; }
    }
}
