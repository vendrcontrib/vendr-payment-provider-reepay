using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    public class ReepaySubscriptionRequest
    {
        [JsonProperty("customer")]
        public string Customer { get; set; }

        [JsonProperty("plan")]
        public string Plan { get; set; }

        [JsonProperty("handle")]
        public string Handle { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("signup_method")]
        public SignupMethod SignupMethod { get; set; }

        [JsonProperty("amount")]
        public int? Amount { get; set; }

        [JsonProperty("quantity")]
        public int? Quantity { get; set; }

        [JsonProperty("amount_incl_vat")]
        public bool? AmountInclVat { get; set; }

        [JsonProperty("create_customer")]
        public ReepayCustomer CreateCustomer { get; set; }

        [JsonProperty("generate_handle")]
        public bool? GenerateHandle { get; set; }

        [JsonProperty("start_date")]
        public string StartDate { get; set; }

        [JsonProperty("end_date")]
        public string EndDate { get; set; }

        [JsonProperty("plan_version")]
        public int? PlanVersion { get; set; }

        [JsonProperty("no_trial")]
        public bool? NoTrial { get; set; }

        [JsonProperty("no_setup_fee")]
        public bool? NoSetupFee { get; set; }

        [JsonProperty("conditional_create")]
        public bool ConditionalCreate { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, string> MetaData { get; set; }
    }
}
