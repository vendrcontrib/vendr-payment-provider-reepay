using Newtonsoft.Json;
using System.Collections.Generic;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    public class ReepayOrder
    {
        [JsonProperty("handle")]
        public string Handle { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("order_lines")]
        public List<ReepayOrderLine> OrderLines { get; set; }

        [JsonProperty("customer")]
        public ReepayCustomer Customer { get; set; }

        [JsonProperty("billing_address")]
        public ReepayAddress BillingAddress { get; set; }

        [JsonProperty("shipping_address")]
        public ReepayAddress ShippingAddress { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, object> MetaData { get; set; }
    }
}
