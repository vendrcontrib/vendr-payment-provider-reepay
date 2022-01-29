using Newtonsoft.Json;
using System;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    /// <summary>
    /// Reepay transaction object: https://reference.reepay.com/api/#transaction
    /// </summary>
    public class ReepayTransaction
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("invoice")]
        public string Invoice { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("settled")]
        public DateTime? Settled { get; set; }

        [JsonProperty("authorized")]
        public DateTime? Authorized { get; set; }

        [JsonProperty("refunded")]
        public DateTime? Refunded { get; set; }

        [JsonProperty("failed")]
        public DateTime? Failed { get; set; }

        [JsonProperty("payment_type")]
        public string PaymentType { get; set; }
    }
}
