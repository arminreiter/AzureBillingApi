using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeHollow.AzureBillingApi;
using CodeHollow.AzureBillingApi.RateCard;
using System.Collections.Generic;
using CodeHollow.AzureBillingApi.Usage;

namespace AzureBillingApi.Tests
{
    [TestClass]
    public class CalculationTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            RateCardData rc = GetRateCardTestData();
            var ud = new UsageData();
            string[] usageData = new string[]
            {
                "1,Test,1,2017-01-12,2017-01-13,13",
                "1,Test,1,2017-01-13,2017-01-14,10",
                "1,Test,1,2017-01-14,2017-01-15,12"
            };
            ud.Values = GetUsageValues(usageData);

            // billing cycle 1: 23 units, 5 for free, 5 with 1.0, rest with 2.0
            // => 5*0 + 5*1.0 * 13*2.0 = 31.0
            // billing cycle 2: 12 units
            // => 5*0 + 5*1.0 + 2*2.0 ) 9.0

            var data = Client.Combine(rc, ud);

            Assert.AreEqual(40.0, data.TotalCosts);
        }

        [TestMethod]
        public void TestMethod2()
        {
            string[] usageData = new string[]
            {
                "1,Test,1,2017-01-12,2017-01-13,3"
            };
            Test(usageData, 0.0);
        }

        [TestMethod]
        public void TestMethod3()
        {
            string[] usageData = new string[]
            {
                "1,Test,1,2017-01-12,2017-01-13,7"
            };
            Test(usageData, 2.0);
        }

        [TestMethod]
        public void TestMethod4()
        {
            string[] usageData = new string[]
            {
                "1,Test,1,2017-01-12,2017-01-13,16"
            };
            Test(usageData, 17.0);
        }

        [TestMethod]
        public void TestMethod5()
        {
            string[] usageData = new string[]
            {
                "1,Test,1,2017-01-12,2017-01-13,1", // + 0.0
                "1,Test,1,2017-01-13,2017-01-14,5", // + 1.0
                "1,Test,1,2017-01-14,2017-01-15,7", // 2* 1.0 (new billing cycle) = 2.0
                "1,Test,1,2017-01-15,2017-01-16,16", // 3*1.0 13*2.0 = 3+26=29

            };
            Test(usageData, 32.0);
        }

        private static void Test(string[] usageData, double expectedResult)
        {
            RateCardData rc = GetRateCardTestData();
            var ud = new UsageData();
            ud.Values = GetUsageValues(usageData);

            var data = Client.Combine(rc, ud);

            Assert.AreEqual(expectedResult, data.TotalCosts);
        }

        private static RateCardData GetRateCardTestData()
        {
            var rc = new RateCardData();
            rc.Currency = "EUR";
            rc.Meters = new List<Meter>();
            rc.Meters.Add(new Meter()
            {
                MeterId = "1",
                IncludedQuantity = 5,
                MeterRates = new Dictionary<double, double>()
                {
                    { 0, 1.0 },
                    { 5, 2.0 }
                }
            });
            return rc;
        }

        private static List<UsageValue> GetUsageValues(string[] usageData)
        {
            List<UsageValue> list = new List<UsageValue>();
            foreach (string d in usageData)
            {
                string[] sp = d.Split(',');
                list.Add(new UsageValue()
                {
                    Id = sp[0],
                    Name = sp[1],
                    Properties = new UsageProperties()
                    { MeterId = sp[2], UsageStartTime = sp[3], UsageEndTime = sp[4], Quantity = int.Parse(sp[5]) }
                });
            }

            return list;
        }
    }
}
