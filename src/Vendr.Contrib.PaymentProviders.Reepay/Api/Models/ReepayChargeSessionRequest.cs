using Newtonsoft.Json;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    public class ReepayChargeSessionRequest : ReepaySessionRequestBase
    {
        [JsonProperty("settle")]
        public bool Settle { get; set; }

        [JsonProperty("order")]
        public ReepayOrder Order { get; set; }

        [JsonProperty("recurring")]
        public bool Recurring { get; set; }
    }
}
