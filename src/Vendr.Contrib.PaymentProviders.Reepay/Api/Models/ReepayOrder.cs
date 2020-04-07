using Newtonsoft.Json;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    public class ReepayOrder
    {
        [JsonProperty("handle")]
        public string Handle { get; set; }

        //[JsonProperty("key")]
        //public string Key { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("customer")]
        public ReepayCustomer Customer { get; set; }
    }
}
