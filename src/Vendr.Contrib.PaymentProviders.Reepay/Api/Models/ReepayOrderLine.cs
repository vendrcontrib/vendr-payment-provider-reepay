using Newtonsoft.Json;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    public class ReepayOrderLine
    {
        [JsonProperty("ordertext")]
        public string OrderText { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("vat")]
        public float VAT { get; set; }
    }
}
