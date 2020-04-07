using Newtonsoft.Json;

namespace Vendr.Contrib.PaymentProviders.Reepay
{
    public class ReepaySessionChargeResult
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
