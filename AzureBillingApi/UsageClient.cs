using CodeHollow.AzureBillingApi.Usage;
using Newtonsoft.Json;
using System;
using System.Net;

namespace CodeHollow.AzureBillingApi
{
    /// <summary>
    /// Reads data from the Azure billing usage REST API.
    /// Uses version 2015-06-01-preview (https://msdn.microsoft.com/en-us/library/azure/mt219003.aspx)
    /// </summary>
    public class UsageClient : Client
    {
        private static readonly string APIVERSION = "2015-06-01-preview";

        /// <summary>
        /// Creates the client to read data from the usage REST API. Uses user authentication for 
        /// authentication. Opens a popup to enter user and password
        /// </summary>
        /// <param name="tenant">the tenant - e.g. mytenant.onmicrosoft.com</param>
        /// <param name="clientId">the client id (application id in the new azure portal)</param>
        /// <param name="subscriptionId">the subscription id</param>
        /// <param name="redirectUrl">the redirect url</param>
        public UsageClient(string tenant, string clientId, string subscriptionId, string redirectUrl)
            : this(tenant, clientId, null, subscriptionId, redirectUrl)
        {
        }

        /// <summary>
        /// Creates the client to read data from the usage  REST API. If clientSecret is empty, it uses
        /// user authentication (popup prompt for user and password). If clientSecret is set, application 
        /// authentication is used
        /// </summary>
        /// <param name="tenant">the tenant - e.g. mytenant.onmicrosoft.com</param>
        /// <param name="clientId">the client id (application id in the new azure portal)</param>
        /// <param name="clientSecret">the client secret (key)</param>
        /// <param name="subscriptionId">the subscription id</param>
        /// <param name="redirectUrl">the redirect url</param>
        public UsageClient(string tenant, string clientId, string clientSecret, string subscriptionId, string redirectUrl)
            : base(tenant, clientId, clientSecret, subscriptionId, redirectUrl)
        {
        }

        /// <summary>
        /// Reads data from the usage REST API.
        /// </summary>
        /// <param name="startDate">Start date - get usage date from this date</param>
        /// <param name="endDate">End date - get usage data to this date</param>
        /// <param name="granularity">The granularity - daily or hourly</param>
        /// <param name="showDetails">Include instance-level details or not</param>
        /// <returns>the usage data for the given time range with the given granularity</returns>
        public UsageData Get(DateTime startDate, DateTime endDate, AggregationGranularity granularity, bool showDetails)
        {
            string token = AzureAuthenticationHelper.GetOAuthTokenFromAAD(Globals.SERVICEURL, Tenant, Globals.RESOURCE, RedirectUrl, ClientId, ClientSecret);
            return Get(startDate, endDate, granularity, showDetails, token);
        }

        /// <summary>
        /// Reads data from the usage REST API.
        /// </summary>
        /// <param name="startDate">Start date - get usage date from this date</param>
        /// <param name="endDate">End date - get usage data to this date</param>
        /// <param name="granularity">The granularity - daily or hourly</param>
        /// <param name="showDetails">Include instance-level details or not</param>
        /// <param name="token">the OAuth token</param>
        /// <returns>the usage data for the given time range with the given granularity</returns>
        public UsageData Get(DateTime startDate, DateTime endDate, AggregationGranularity granularity, bool showDetails, string token)
        {
            if (startDate >= endDate)
                throw new ArgumentException("Start date must be before the end date!");

            if (endDate >= DateTime.Now.AddHours(-1))
            {
                endDate = DateTime.Now.AddHours(-1).ToUniversalTime();
            }

            DateTimeOffset startTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0, DateTimeKind.Utc);
            DateTimeOffset endTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0, DateTimeKind.Utc);
            
            if (granularity == AggregationGranularity.Hourly)
            {
                startTime = startTime.AddHours(startDate.Hour);
                endTime = endTime.AddHours(endDate.Hour);
            }

            string st = WebUtility.UrlEncode(startTime.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            string et = WebUtility.UrlEncode(endTime.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            string url = $"https://management.azure.com/subscriptions/{SubscriptionId}/providers/Microsoft.Commerce/UsageAggregates?api-version={APIVERSION}&reportedStartTime={st}&reportedEndTime={et}&aggregationGranularity={granularity.ToString()}&showDetails={showDetails.ToString().ToLower()}";
            
            string data = GetData(url, token);
            if (String.IsNullOrEmpty(data))
                return null;

            var usageData = JsonConvert.DeserializeObject<UsageData>(data);

            // read data from the usagedata api as long as the continuationtoken is set.
            // usage data api returns 1000 values per api call, to receive all values,  
            // we have to call the url stored in nextLink property.
            while (!String.IsNullOrEmpty(usageData.NextLink))
            {
                string next = GetData(usageData.NextLink, token);
                var nextUsageData = JsonConvert.DeserializeObject<UsageData>(next);
                usageData.Values.AddRange(nextUsageData.Values);
                usageData.NextLink = nextUsageData.NextLink;
            }

            return usageData;
        }
    }
}
