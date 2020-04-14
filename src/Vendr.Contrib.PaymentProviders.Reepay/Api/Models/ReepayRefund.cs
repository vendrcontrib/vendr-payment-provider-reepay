using Newtonsoft.Json;
using System;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    public class ReepayRefund
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("invoice")]
        public string Invoice { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("transaction")]
        public string Transaction { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_state")]
        public string ErrorState { get; set; }

        [JsonProperty("acquirer_message")]
        public string AcquirerMessage { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("credit_note_id")]
        public string CreditNoteId { get; set; }

        [JsonProperty("ref_transaction")]
        public string RefTransaction { get; set; }
    }
}
