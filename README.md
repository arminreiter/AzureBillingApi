# AzureBillingApi
.net library (.net standard 1.4 and .net framework 4.5.2) that reads data from the azure rest billing apis. It can be used to:
 - Read data from the Azure billing **RateCard API**
 - Read data from the Azure billing **Usage API**
 - Read data from both APIs and combine them to calculate the costs (without tax).

This library is also available as **NuGet Package** (https://www.nuget.org/packages/CodeHollow.AzureBillingApi/)

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

### Get usage and ratecard data and combine them:
```csharp
Client c = new Client("mytenant.onmicrosoft.com", "[CLIENTID]",
    "[CLIENTSECRET]", "[SUBSCRIPTIONID]", "http://[REDIRECTURL]");

var usageData = c.GetUsageData(new DateTime(2017, 1, 14, 0, 0, 0), DateTime.Now, Usage.AggregationGranularity.Daily, true);
var ratecardData = c.GetRateCardData("MS-AZR-0003p", "EUR", "de-AT", "AT");
var combined = Client.Combine(ratecardData, usageData);

var costs = from x in combined.Costs select x.CalculatedCosts;
var totalCosts = costs.Sum();
```

### Get usage and costs for a period
This can be achieved with the build in method *GetResourceCostsForPeriod*:

```csharp
Client c = new Client("mytenant.onmicrosoft.com", "[CLIENTID]",
    "[CLIENTSECRET]", "[SUBSCRIPTIONID]", "http://[REDIRECTURL]");
var resourceData = c.GetResourceCostsForPeriod("MS-AZR-0017P", "EUR", "en-US", "AT", 2017, 1);
```
This method could be **slow**, because the correct values for one billing cycle (so that it's the same as on the bill) are determined with the parameters:
- Start date: e.g. 14.01.2017 00:00
- End date: e.g. 13.02.2017 23:00
- Aggregation granularity: **Hourly**

That's the reason why *GetResourceCostsForPeriod* reads the usage value with hourly granularity which takes some time. You could use the GetResourceCosts with Daily aggregation to speed it up, but the resulting costs per meter/resource/totalcosts will not be the exact same as on the bill:

```csharp
Client c = new Client("mytenant.onmicrosoft.com", "[CLIENTID]", "[CLIENTSECRET]", "[SUBSCRIPTIONID]", "http://[REDIRECTURL]");
var resourceCosts = c.GetResourceCosts("MS-AZR-0003p", "EUR", "en-US", "AT",
            new DateTime(2017, 1, 14), new DateTime(2017, 2, 13), AggregationGranularity.Daily, true);
```

## Improvements (todo)
* Documentation
   * Troubleshoot section in readme file
* Add tax parameter