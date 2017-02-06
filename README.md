# AzureBillingApi
.net library that reads data from the azure rest billing apis.

Currently works best if start and end date are the same as the billing period (cycle). If it's not, then the cost calculation could be wrong because of the included units and different meter rates.

***more detailed description will follow***

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

## Configuration of Azure Active Directory
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


## TODO
1. Billing Period (cycle)
   * Billing period (billing cycle) is not taken into account. That's relevant if you want the correct costs and the start and end date are not the same as the billing period.
   * Sample: The billing period is from 14.11.2016 - 13.12.2016
      * if you read the data from 14.10.2016 - 13.12.2016 (2 periods) - then the meter rates and included quantity is taken for the whole timespan - so for 2 periods.
      * if 5 GB data transfer is for free and you have 4 GB in each period - then it should be free. But in the given timespan (2 periods) - it's summed up to 8 GB and the library returns the costs for 3 GB.
2. Documentation
   * Troubleshoot section in readme file
   * blog post
   * configuration of active directory
   * parameters in Console
   * billing period as parameters
3. NuGet package
4. Code improvements
5. ... 