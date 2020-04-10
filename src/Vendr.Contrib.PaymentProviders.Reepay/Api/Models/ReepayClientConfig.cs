namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    public class ReepayClientConfig
    {
        public string BaseUrl { get; set; }
        public string Authorization { get; set; }
        public string WebhookSecret { get; set; }
    }
}
