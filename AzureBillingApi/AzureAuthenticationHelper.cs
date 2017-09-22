using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;

namespace CodeHollow.AzureBillingApi
{
    /// <summary>
    /// Helps to authenticate (get the OAuth token) at Azure.
    /// </summary>
    public static class AzureAuthenticationHelper
    {
        /// <summary>
        /// Authenticates to Azure AD and returns the OAuth token. If clientSecret is provided, authentication is done 
        /// via app authentication. If clientSecret is not provided, authentication is done with user prompt.
        /// </summary>
        /// <param name="serviceurl">the serviceurl - e.g. https://login.microsoftonline.com</param>
        /// <param name="tenant">the full tenant - e.g. mytenant.onmicrosoft.com</param>
        /// <param name="resource">the resource url - e.g. https://management.azure.com/ </param>
        /// <param name="redirectUrl">the redirect url - e.g. http://localhost/myApp </param>
        /// <param name="clientId">the client id which is called "application id" in the new azure portal - e.g. 319730c5-4b39-4c3a-a488-24c0226d9240</param>
        /// <param name="clientSecret">the client secret - leave it empty for user authentication</param>
        /// <returns></returns>
        public static string GetOAuthTokenFromAAD(string serviceurl, string tenant, string resource, string redirectUrl, string clientId, string clientSecret = null)
        {
            AuthenticationResult result;

            if (String.IsNullOrEmpty(clientSecret)) // if no client secret - authenticate with user...
                result = GetOAuthTokenForUser(serviceurl, tenant, resource, redirectUrl, clientId);
            else                                    // else authenticate with application
                result = GetOAuthTokenForApplication(serviceurl, tenant, resource, redirectUrl, clientId, clientSecret);

            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }

        private static AuthenticationResult GetOAuthTokenForUser(string url, string tenant, string resource, string redirectUrl, string clientId)
        {
            var authenticationContext = new AuthenticationContext(CombineUrl(url, tenant));

#if NET45
            PlatformParameters p = new PlatformParameters(PromptBehavior.Auto);
#else
            PlatformParameters p = new PlatformParameters();
#endif

            var authTask = authenticationContext.AcquireTokenAsync(resource, clientId, new Uri(redirectUrl), p);
            authTask.Wait();
            return authTask.Result;
        }

        private static AuthenticationResult GetOAuthTokenForApplication(string url, string tenant, string resource, string redirectUrl, string clientId, string clientSecret)
        {
            var authenticationContext = new AuthenticationContext(CombineUrl(url, tenant));
            ClientCredential clientCred = new ClientCredential(clientId, clientSecret);
            var authTask = authenticationContext.AcquireTokenAsync(resource, clientCred);
            authTask.Wait();
            return authTask.Result;
        }

        private static string CombineUrl(string rootUrl, string relativeUrl)
        {
            Uri rootUri = new Uri(rootUrl);
            Uri combined = new Uri(rootUri, relativeUrl);
            return combined.ToString();
        }
    }
}
