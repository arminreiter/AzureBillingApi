# AzureBillingApi
.net library that reads data from the azure rest billing apis. It can be used to:
 - Read data from the Azure billing *RateCard API*
 - Read data from the Azure billing *Usage API*
 - Read data from both APIs and combine them to calculate the costs (without tax).

This library is also available as *NuGet Package* (https://www.nuget.org/packages/CodeHollow.AzureBillingApi/)

A detailed description can be found at my blog: https://codehollow.com/2017/02/using-the-azure-billing-api-to-calculate-the-costs/

## Prerequisits: Configuration of Azure Active Directory
1. Add a new application to active directory: 
   * Open https://portal.azure.com/, navigate to the Azure Active Directory and App Registrations
   * Add a new **native** application
2. Add delegated permissions for Windows Azure Service Management
   * App registrations - Settings - Required Permissions - Add - Windows Azure Service Management API
3. Copy the Application ID and use it as the client id
   * If you use application authentication, create a new key and use it as client secret
4. Give the user/application at least "Reader" rights for the subscription
   * Subscriptions - your subscriptions - Access control (IAM)
5. Build and run the application

## Usage
### Getting the costs (combination of usage data and ratecard data)
```csharp
Client c = new Client("mytenant.onmicrosoft.com", "[CLIENTID]",
    "[CLIENTSECRET]", "[SUBSCRIPTIONID]", "http://[REDIRECTURL]");

var resourceCosts = c.GetResourceCosts("MS-AZR-0003p", "EUR", "en-US", "AT",
            new DateTime(2016, 11, 14, 0, 0, 0), new DateTime(2016, 12, 13, 23, 0, 0), AggregationGranularity.Hourly, true);

var costs = from x in resourceCosts.Costs select x.CalculatedCosts;
var totalCosts = costs.Sum();

Console.WriteLine(totalCosts + " " + resourceCosts.RateCardData.Currency);
```

### Getting only the usage data
```csharp
Client c = new Client("mytenant.onmicrosoft.com", "[CLIENTID]",
    "[CLIENTSECRET]", "[SUBSCRIPTIONID]", "http://[REDIRECTURL]");

var usageData = c.GetUsageData(new DateTime(2017, 1, 14, 0, 0, 0), DateTime.Now, Usage.AggregationGranularity.Daily, true);
foreach(var usageValue in usageData.Values)
{
    Console.Write(usageValue.Name + ": " + usageValue.Properties.UsageStartTime + " - " + usageValue.Properties.UsageEndTime + " - " + usageValue.Properties.Quantity);
}
```

### Getting only the ratecard data
```csharp
Client c = new Client("mytenant.onmicrosoft.com", "[CLIENTID]",
    "[CLIENTSECRET]", "[SUBSCRIPTIONID]", "http://[REDIRECTURL]");

var ratecardData = c.GetRateCardData("MS-AZR-0003p", "EUR", "de-AT", "AT");
foreach(var meter in ratecardData.Meters)
{
    Console.WriteLine(meter.MeterName + ": " + meter.MeterRates.First().Value);
}
```


## TODO
* Documentation
   * Troubleshoot section in readme file
   * parameters in sample console
   * billing period as parameters
* Add tax parameter
* Add unit test
* Code improvements