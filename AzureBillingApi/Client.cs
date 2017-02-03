using CodeHollow.AzureBillingApi.Usage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CodeHollow.AzureBillingApi
{
    /// <summary>
    /// Client to read data from the Azure billing REST APIs.
    /// Allows to get data from the usage api <see cref="UsageClient"/> or from
    /// the ratecard api <see cref="RateCardClient"/>.
    /// </summary>
    public class Client
    {
        protected string Tenant { get; set; }
        protected string ClientId { get; set; }
        protected string ClientSecret { get; set; }
        protected string SubscriptionId { get; set; }
        protected string RedirectUrl { get; set; }

        /// <summary>
        /// Creates the client to read data from the billing REST APIs. Uses user authentication for 
        /// authentication. Opens a popup to enter user and password
        /// </summary>
        /// <param name="tenant">the tenant - e.g. mytenant.onmicrosoft.com</param>
        /// <param name="clientId">the client id (application id in the new azure portal)</param>
        /// <param name="subscriptionId">the subscription id</param>
        /// <param name="redirectUrl">the redirect url</param>
        public Client(string tenant, string clientId, string subscriptionId, string redirectUrl)
            : this(tenant, clientId, null, subscriptionId, redirectUrl)
        {
        }

        /// <summary>
        /// Creates the client to read data from the billing REST APIs. If clientSecret is empty, it uses
        /// user authentication (popup prompt for user and password). If clientSecret is set, application 
        /// authentication is used
        /// </summary>
        /// <param name="tenant">the tenant - e.g. mytenant.onmicrosoft.com</param>
        /// <param name="clientId">the client id (application id in the new azure portal)</param>
        /// <param name="clientSecret">the client secret (key)</param>
        /// <param name="subscriptionId">the subscription id</param>
        /// <param name="redirectUrl">the redirect url</param>
        public Client(string tenant, string clientId, string clientSecret, string subscriptionId, string redirectUrl)
        {
            Tenant = tenant;
            ClientId = clientId;
            ClientSecret = clientSecret;
            SubscriptionId = subscriptionId;
            RedirectUrl = redirectUrl;
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
        public Usage.UsageData GetUsageData(DateTime startDate, DateTime endDate, AggregationGranularity granularity, bool showDetails, string token = null)
        {
            var uclient = new UsageClient(Tenant, ClientId, ClientSecret, SubscriptionId, RedirectUrl);
            if (string.IsNullOrEmpty(token))
                return uclient.Get(startDate, endDate, granularity, showDetails);

            return uclient.Get(startDate, endDate, granularity, showDetails, token);
        }

        /// <summary>
        /// Reads the ratecard data from the REST API with an existing OAuth token. Useful if you want to minimize 
        /// the authorizations
        /// </summary>
        /// <param name="offerDurableId">Offer - e.g. MS-AZR-0003p (see: https://azure.microsoft.com/en-us/support/legal/offer-details/) </param>
        /// <param name="currency">the currency - e.g. EUR or USD</param>
        /// <param name="locale">the locale - e.g. de-AT or en-US</param>
        /// <param name="regionInfo">the region - e.g. DE, AT or US</param>
        /// <param name="token">the OAuth token</param>
        /// <returns>The ratecard information</returns>
        public RateCard.RateCardData GetRateCardData(string offerDurableId, string currency, string locale, string regionInfo, string token = null)
        {
            var rcclient = new RateCardClient(Tenant, ClientId, ClientSecret, SubscriptionId, RedirectUrl);
            if (string.IsNullOrEmpty(token))
                return rcclient.Get(offerDurableId, currency, locale, regionInfo);

            return rcclient.Get(offerDurableId, currency, locale, regionInfo, token);
        }

        /// <summary>
        /// Returns the data of the billing apis (ratecard and usage) connected and 
        /// calculates the costs.
        /// </summary>
        /// <param name="offerDurableId">Offer - e.g. MS-AZR-0003p (see: https://azure.microsoft.com/en-us/support/legal/offer-details/) </param>
        /// <param name="currency">the currency - e.g. EUR or USD</param>
        /// <param name="locale">the locale - e.g. de-AT or en-US</param>
        /// <param name="regionInfo">the region - e.g. DE, AT or US</param>
        /// <param name="startDate">Start date - get usage date from this date</param>
        /// <param name="endDate">End date - get usage data to this date</param>
        /// <param name="granularity">The granularity - daily or hourly</param>
        /// <param name="showDetails">Include instance-level details or not</param>
        /// <param name="token">the OAuth token</param>
        /// <returns>The costs of the resources (combined data of ratecard and usage api)</returns>
        public List<ResourceCosts> GetResourceCosts(string offerDurableId, string currency, string locale, string regionInfo, DateTime startDate, DateTime enddate, AggregationGranularity granularity, bool showDetail, string token = null)
        {
            if(string.IsNullOrEmpty(token))
            {
                token = AzureAuthenticationHelper.GetOAuthTokenFromAAD(Globals.SERVICEURL, Tenant, Globals.RESOURCE, RedirectUrl, ClientId, ClientSecret);
            }
            var rateCardData = GetRateCardData(offerDurableId, currency, locale, regionInfo, token);
            var usageData = GetUsageData(startDate, enddate, granularity, showDetail, token);
            return Combine(rateCardData, usageData);
        }

        /// <summary>
        /// Combines the ratecard data with the usage data.
        /// </summary>
        /// <param name="rateCardData">RateCard data</param>
        /// <param name="usageData">Usage data</param>
        /// <returns>The costs of the resources (combined data of ratecard and usage api)</returns>
        public static List<ResourceCosts> Combine(RateCard.RateCardData rateCardData, Usage.UsageData usageData)
        {
            List<ResourceCosts> costs = new List<ResourceCosts>();

            foreach (var usageValue in usageData.Value)
            {
                string meterId = usageValue.Properties.MeterId;
                var v = from x in rateCardData.Meters where x.MeterId == meterId select x;
                var meter = v.First();

                costs.Add(new ResourceCosts()
                {
                    RateCardMeter = meter,
                    UsageValue = usageValue,
                    Costs = usageValue.Properties.Quantity * meter.MeterRates.First().Value
                });
            }
            return costs;
        }

        /// <summary>
        /// Reads data from a url including the oauth token.
        /// </summary>
        /// <param name="url">service url</param>
        /// <param name="token">oauth token</param>
        /// <returns></returns>
        protected static string GetData(string url, string token)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = client.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                string errorMsg = "An error occurred! The service returned: " + response.ToString();

                var x = response.Content.ReadAsStringAsync();
                x.Wait();
                errorMsg += "Content: " + x.Result;
                throw new Exception(errorMsg); // TODO: create own exception
            }

            var readTask = response.Content.ReadAsStringAsync();
            readTask.Wait();
            return readTask.Result;
        }
    }
}
