using Newtonsoft.Json;
using System;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    /// <summary>
    /// Reepay charge object: https://reference.reepay.com/api/#charge
    /// </summary>
    public class ReepayCharge
    {
        [JsonProperty("handle")]
        public string Handle { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("customer")]
        public string Customer { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("authorized")]
        public DateTime? Authorized { get; set; }

        [JsonProperty("settled")]
        public DateTime? Settled { get; set; }

        [JsonProperty("cancelled")]
        public DateTime? Cancelled { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("transaction")]
        public string Transaction { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_state")]
        public string ErrorState { get; set; } 

        [JsonProperty("processing")]
        public bool Processing { get; set; }

        [JsonProperty("source")]
        public ReepaySource Source { get; set; }

        [JsonProperty("refunded_amount")]
        public int RefundedAmount { get; set; }

        [JsonProperty("authorized_amount")]
        public int AuthorizedAmount { get; set; }
    }
}
