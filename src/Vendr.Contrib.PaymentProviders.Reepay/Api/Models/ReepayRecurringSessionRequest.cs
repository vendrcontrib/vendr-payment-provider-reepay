using Newtonsoft.Json;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    public class ReepayRecurringSessionRequest : ReepaySessionRequestBase
    {
        [JsonProperty("create_customer")]
        public ReepayCustomer CreateCustomer { get; set; }
    }
}
