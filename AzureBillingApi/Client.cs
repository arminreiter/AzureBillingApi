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
    /// Allows to get data from the usage api <see cref="UsageClient"/>, from
    /// the ratecard api <see cref="RateCardClient"/> or get the combination of
    /// the usage and ratecard data <see cref="GetResourceCosts(string, string, string, string, DateTime, DateTime, AggregationGranularity, bool, string)"/>.
    /// Requires registration of an application in the active directory configuration in the azure portal.
    /// Please check https://github.com/codehollow/AzureBillingApi for more details.
    /// </summary>
    public class Client
    {
        #region Properties

        /// <summary>
        /// The tenant - e.g. mytenant.onmicrosoft.com
        /// </summary>
        protected string Tenant { get; set; }

        /// <summary>
        /// The client id (application id in the new azure portal)
        /// </summary>
        protected string ClientId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected string ClientSecret { get; set; }

        /// <summary>
        /// The subscription id
        /// </summary>
        protected string SubscriptionId { get; set; }

        /// <summary>
        /// The redirect url
        /// </summary>
        protected string RedirectUrl { get; set; }

        #endregion

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
        public ResourceCostData GetResourceCosts(string offerDurableId, string currency, string locale, string regionInfo, DateTime startDate, DateTime endDate, AggregationGranularity granularity, bool showDetails, string token = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                token = AzureAuthenticationHelper.GetOAuthTokenFromAAD(Globals.SERVICEURL, Tenant, Globals.RESOURCE, RedirectUrl, ClientId, ClientSecret);
            }
            var rateCardData = GetRateCardData(offerDurableId, currency, locale, regionInfo, token);

            // set startdate to the beginning of the period so that the cost calculation will be correct.
            DateTime start = GetStartOfBillingCycle(startDate);

            var usageData = GetUsageData(start, endDate, granularity, showDetails, token);
            var calculated = Combine(rateCardData, usageData);

            calculated.Costs = calculated.Costs.Where(x => x.UsageValue.Properties.UsageStartTimeAsDate >= startDate).ToList();

            return calculated;
        }

        /// <summary>
        /// Returns the usage and costs for one billing period (cycle). Billing cycle is automatically set to
        /// e.g. 14.12.2016 - 13.1.2017 23:59. 
        /// </summary>
        /// <param name="offerDurableId">Offer - e.g. MS-AZR-0003p (see: https://azure.microsoft.com/en-us/support/legal/offer-details/) </param>
        /// <param name="currency">the currency - e.g. EUR or USD</param>
        /// <param name="locale">the locale - e.g. de-AT or en-US</param>
        /// <param name="regionInfo">the region - e.g. DE, AT or US</param>
        /// <param name="yearOfBeginningPeriod">year of the beginning of the billing cycle - e.g. 2016</param>
        /// <param name="monthOfBeginningPeriod">month of the beginning of the billing cycle - e.g. 12</param>
        /// <param name="token">the OAuth token</param>
        /// <returns>The costs of the resources for one billing period (one month)</returns>
        public ResourceCostData GetResourceCostsForPeriod(string offerDurableId, string currency, string locale, string regionInfo, int yearOfBeginningPeriod, int monthOfBeginningPeriod, string token = null)
        {
            DateTime startDate = new DateTime(yearOfBeginningPeriod, monthOfBeginningPeriod, 14, 0,0,0, DateTimeKind.Utc);
            if (startDate > DateTime.Now.ToUniversalTime())
                throw new ArgumentException("The beginning of the period can not be in the future!");
            DateTime endDate = startDate.AddMonths(1).AddHours(-1);

            if(endDate > DateTime.Now.ToUniversalTime())
            {
                endDate = DateTime.Now.ToUniversalTime();
            }

            return GetResourceCosts(offerDurableId, currency, locale, regionInfo, startDate, endDate, AggregationGranularity.Hourly, true, token);
        }

        /// <summary>
        /// Combines the ratecard data with the usage data.
        /// </summary>
        /// <param name="rateCardData">RateCard data</param>
        /// <param name="usageData">Usage data</param>
        /// <returns>The costs of the resources (combined data of ratecard and usage api)</returns>
        public static ResourceCostData Combine(RateCard.RateCardData rateCardData, Usage.UsageData usageData)
        {
            ResourceCostData rcd = new ResourceCostData();
            rcd.Costs = new List<ResourceCosts>();
            rcd.RateCardData = rateCardData;
            
            // get all used meter ids
            var meterIds = (from x in usageData.Values select x.Properties.MeterId).Distinct().ToList();

            // aggregates all quantity and will be used to calculate costs (e.g. if quantity is included for free)
            Dictionary<string, double> aggregatedQuantity = meterIds.ToDictionary(x => x, x => 0.0);

            foreach (var usageValue in usageData.Values)
            {
                string meterId = usageValue.Properties.MeterId;
                var rateCard = (from x in rateCardData.Meters where x.MeterId == meterId select x).First();

                var usedQuantity = aggregatedQuantity[meterId];

                var curcosts = GetMeterRate(rateCard.MeterRates, rateCard.IncludedQuantity, usedQuantity, usageValue.Properties.Quantity);

                aggregatedQuantity[meterId] += usageValue.Properties.Quantity;

                rcd.Costs.Add(new ResourceCosts()
                {
                    RateCardMeter = rateCard,
                    UsageValue = usageValue,
                    CalculatedCosts = curcosts.Item1,
                    BillableUnits = curcosts.Item2
                });
            }

            return rcd;
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
                throw new Exception(errorMsg);
            }

            var readTask = response.Content.ReadAsStringAsync();
            readTask.Wait();
            return readTask.Result;
        }
        
        /// <summary>
        /// Returns the costs for the given quantityToAdd for one billing cycle.
        /// </summary>
        /// <param name="meterRates">List of the meter rates</param>
        /// <param name="includedQuantity">Amount of included quantity (which is for free)</param>
        /// <param name="totalUsedQuantity">The already use quantity in the period</param>
        /// <param name="quantityToAdd">the quantity for which the calculation should be done</param>
        /// <returns>Tuple - Item1 = costs, Item2 = billableQuantity</returns>
        private static Tuple<double, double> GetMeterRate(Dictionary<double, double> meterRates, double includedQuantity, double totalUsedQuantity, double quantityToAdd)
        {
            Dictionary<double, double> modifiedMeterRates;

            // add included quantity to meter rates with cost 0
            if (includedQuantity > 0)
            {
                modifiedMeterRates = new Dictionary<double, double>();
                modifiedMeterRates.Add(0, 0);

                foreach (var rate in meterRates)
                {
                    modifiedMeterRates.Add(rate.Key + includedQuantity, rate.Value);
                }
            }
            else
                modifiedMeterRates = meterRates;
            
            double costs = 0.0;
            double billableQuantity = 0.0;

            for (int i = modifiedMeterRates.Count; i > 0; i--)
            {
                var totalNew = totalUsedQuantity + quantityToAdd;
                var rate = modifiedMeterRates.ElementAt(i - 1);

                var tmp = quantityToAdd - rate.Key;

                if (tmp > 0)
                {
                    costs += tmp * rate.Value;
                    if (rate.Value > 0)
                        billableQuantity += tmp;
                    quantityToAdd -= tmp;
                }
            }
            return new Tuple<double, double>(costs, billableQuantity);
        }

        /// <summary>
        /// Returns the start of the billing cycle (the 14th of the month)
        /// </summary>
        /// <param name="startDate">Date from which the billing cycle beginning should be calculated</param>
        /// <returns>Beginning of the billing cycle</returns>
        private static DateTime GetStartOfBillingCycle(DateTime startDate)
        {
            // start is per default the 14th of the month
            if (startDate.Day == 14)
                return startDate;

            if (startDate.Day < 14)
                startDate = startDate.AddMonths(-1);

            return startDate.AddDays(14 - startDate.Day);
        }

    }
}
