using Newtonsoft.Json;
using System;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    /// <summary>
    /// Reepay subscription object: https://reference.reepay.com/api/#subscription
    /// </summary>
    public class ReepaySubscription
    {
        [JsonProperty("handle")]
        public string Handle { get; set; }

        [JsonProperty("customer")]
        public string Customer { get; set; }

        [JsonProperty("plan")]
        public string Plan { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("test")]
        public bool Test { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("expires")]
        public string Expires { get; set; }

        [JsonProperty("reactivated")]
        public string Reactivated { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("activated")]
        public DateTime? Activated { get; set; }

        [JsonProperty("renewing")]
        public bool Renewing { get; set; }

        [JsonProperty("plan_version")]
        public int PlanVersion { get; set; }

        [JsonProperty("amount_incl_vat")]
        public bool? AmountInclVat { get; set; }

        [JsonProperty("start_date")]
        public DateTime StartDate { get; set; }

        [JsonProperty("end_date")]
        public DateTime? EndDate { get; set; }

        [JsonProperty("is_cancelled")]
        public bool IsCancelled { get; set; }

        [JsonProperty("in_trial")]
        public bool InTrial { get; set; }

        [JsonProperty("has_started")]
        public bool HasStarted { get; set; }

        [JsonProperty("renewal_count")]
        public int RenewalCount { get; set; }
    }
}