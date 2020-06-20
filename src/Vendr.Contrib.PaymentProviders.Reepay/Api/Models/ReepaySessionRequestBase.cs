using Newtonsoft.Json;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    public class ReepaySessionRequestBase
    {
        [JsonProperty("configuration")]
        public string Configuration { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("ttl")]
        public string TTL { get; set; }

        [JsonProperty("accept_url")]
        public string AcceptUrl { get; set; }

        [JsonProperty("cancel_url")]
        public string CancelUrl { get; set; }

        [JsonProperty("payment_methods")]
        public string[] PaymentMethods { get; set; }

        [JsonProperty("button_text")]
        public string ButtonText { get; set; }
    }
}
