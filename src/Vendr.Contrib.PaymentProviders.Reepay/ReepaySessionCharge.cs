using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Vendr.Contrib.PaymentProviders.Reepay
{
    public class ReepaySessionCharge
    {
        [JsonProperty("configuration")]
        public string Configuration { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("settle")]
        public bool Settle { get; set; }

        [JsonProperty("order")]
        public ReepayOrder Order { get; set; }

        [JsonProperty("recurring")]
        public bool Recurring { get; set; }

        [JsonProperty("accept_url")]
        public string AcceptUrl { get; set; }

        [JsonProperty("cancel_url")]
        public string CancelUrl { get; set; }

        [JsonProperty("payment_methods")]
        public string[] PaymentMethods { get; set; }
    }
}
