using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    /// <summary>
    /// Reepay webhook: https://reference.reepay.com/api/#webhooks
    /// </summary>
    public class ReepayWebhookEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("event_id")]
        public string EventId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("event_type")]
        public WebhookEventType EventType { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("customer")]
        public string Customer { get; set; }

        [JsonProperty("payment_method")]
        public string PaymentMethod { get; set; }

        [JsonProperty("subscription")]
        public string Subscription { get; set; }

        [JsonProperty("invoice")]
        public string Invoice { get; set; }

        [JsonProperty("transaction")]
        public string Transaction { get; set; }

        [JsonProperty("credit_note")]
        public string CreditNote { get; set; }

        [JsonProperty("credit")]
        public string Credit { get; set; }
    }
}
