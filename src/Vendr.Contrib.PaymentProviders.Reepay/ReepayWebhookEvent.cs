using System;
using System.Runtime.Serialization;

namespace Vendr.Contrib.PaymentProviders.Reepay
{
    /// <summary>
    /// Reepay webhook object: https://reference.reepay.com/api/#the-webhook-object
    /// </summary>
    [DataContract]
    public class ReepayWebhookEvent
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "event")]
        public string Event { get; set; }

        [DataMember(Name = "state")]
        public string State { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }

        [DataMember(Name = "content")]
        public string Content { get; set; }

        [DataMember(Name = "created")]
        public DateTime Created { get; set; }

        [DataMember(Name = "count")]
        public int Count { get; set; }

        [DataMember(Name = "last_fail")]
        public DateTime? LastFail { get; set; }

        [DataMember(Name = "first_fail")]
        public DateTime? FirstFail { get; set; }

        [DataMember(Name = "alert_count")]
        public int? AlertCount { get; set; }

        [DataMember(Name = "alert_sent")]
        public DateTime? AlertSent { get; set; }
    }
}
