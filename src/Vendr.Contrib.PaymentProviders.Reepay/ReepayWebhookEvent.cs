using System;
using System.Runtime.Serialization;

namespace Vendr.Contrib.PaymentProviders.Reepay
{
    /// <summary>
    /// Reepay webhook object: https://reference.reepay.com/api/#the-webhook-object
    /// </summary>
    [DataContract]
    public class ReepayWebhookEvent : ReepayEvent
    {
        [DataMember(Name = "event_id")]
        public string EventId { get; set; }

        [DataMember(Name = "timestamp")]
        public DateTime Timestamp { get; set; }

        [DataMember(Name = "signature")]
        public string Signature { get; set; }

        [DataMember(Name = "payment_method")]
        public string PaymentMethod { get; set; }

        [DataMember(Name = "transaction")]
        public string Transaction { get; set; }

        [DataMember(Name = "credit_note")]
        public string CreditNote { get; set; }

        [DataMember(Name = "credit")]
        public string Credit { get; set; }
    }
}
