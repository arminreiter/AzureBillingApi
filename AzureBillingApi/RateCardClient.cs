using CodeHollow.AzureBillingApi.RateCard;
using Newtonsoft.Json;

namespace CodeHollow.AzureBillingApi
{
    /// <summary>
    /// Reads data from the Azure billing ratecard REST API. 
    /// Uses version 2016-08-31-preview (https://msdn.microsoft.com/en-us/library/azure/mt219004.aspx)
    /// </summary>
    public class RateCardClient : Client
    {
        private static readonly string APIVERSION = "2016-08-31-preview"; // "2015-06-01-preview";

        /// <summary>
        /// Creates the client to read data from the ratecard REST API. Uses user authentication for 
        /// authentication. Opens a popup to enter user and password
        /// </summary>
        /// <param name="tenant">the tenant - e.g. mytenant.onmicrosoft.com</param>
        /// <param name="clientId">the client id (application id in the new azure portal)</param>
        /// <param name="subscriptionId">the subscription id</param>
        /// <param name="redirectUrl">the redirect url</param>
        public RateCardClient(string tenant, string clientId, string subscriptionId, string redirectUrl)
            : this(tenant, clientId, null, subscriptionId, redirectUrl)
        {
        }

        /// <summary>
        /// Creates the client to read data from the ratecard REST API. If clientSecret is empty, it uses
        /// user authentication (popup prompt for user and password). If clientSecret is set, application 
        /// authentication is used
        /// </summary>
        /// <param name="tenant">the tenant - e.g. mytenant.onmicrosoft.com</param>
        /// <param name="clientId">the client id (application id in the new azure portal)</param>
        /// <param name="clientSecret">the client secret (key)</param>
        /// <param name="subscriptionId">the subscription id</param>
        /// <param name="redirectUrl">the redirect url</param>
        public RateCardClient(string tenant, string clientId, string clientSecret, string subscriptionId, string redirectUrl)
            : base(tenant, clientId, clientSecret, subscriptionId, redirectUrl)
        {
        }

        /// <summary>
        /// Reads the ratecard data from the REST API.
        /// </summary>
        /// <param name="offerDurableId">Offer - e.g. MS-AZR-0003p (see: https://azure.microsoft.com/en-us/support/legal/offer-details/) </param>
        /// <param name="currency">the currency - e.g. EUR or USD</param>
        /// <param name="locale">the locale - e.g. de-AT or en-US</param>
        /// <param name="regionInfo">the region - e.g. DE, AT or US</param>
        /// <returns>The ratecard information</returns>
        public RateCardData Get(string offerDurableId, string currency, string locale, string regionInfo)
        {
            string token = AzureAuthenticationHelper.GetOAuthTokenFromAAD(Globals.SERVICEURL, Tenant, Globals.RESOURCE, RedirectUrl, ClientId, ClientSecret);
            return Get(offerDurableId, currency, locale, regionInfo, token);
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
        public RateCardData Get(string offerDurableId, string currency, string locale, string regionInfo, string token)
        {
            string url = $"https://management.azure.com/subscriptions/{SubscriptionId}/providers/Microsoft.Commerce/RateCard?api-version={APIVERSION}&$filter=OfferDurableId eq '{offerDurableId}' and Currency eq '{currency}' and Locale eq '{locale}' and RegionInfo eq '{regionInfo}'";

            var data = GetData(url, token);
            
            return JsonConvert.DeserializeObject<RateCardData>(data);
        }
    }
}
