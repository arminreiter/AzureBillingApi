namespace CodeHollow.AzureBillingApi
{
    public class ResourceCosts
    {
        public Usage.UsageValue UsageValue { get; set; }
        public RateCard.Meter RateCardMeter { get; set; }
        public double Costs { get; set; }
    }
}
