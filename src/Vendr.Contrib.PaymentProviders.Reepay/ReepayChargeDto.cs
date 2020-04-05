using System;
using System.Runtime.Serialization;

namespace Vendr.Contrib.PaymentProviders.Reepay
{
    /// <summary>
    /// Reepay charge object: https://reference.reepay.com/api/#charge
    /// </summary>
    [DataContract]
    public class ReepayChargeDto
    {
        [DataMember(Name = "handle")]
        public string Handle { get; set; }

        [DataMember(Name = "state")]
        public string State { get; set; }

        [DataMember(Name = "customer")]
        public string Customer { get; set; }

        [DataMember(Name = "amount")]
        public int Amount { get; set; }

        [DataMember(Name = "currency")]
        public string Currency { get; set; }

        [DataMember(Name = "authorized")]
        public DateTime? Authorized { get; set; }

        [DataMember(Name = "settled")]
        public DateTime? Settled { get; set; }

        [DataMember(Name = "cancelled")]
        public DateTime? Cancelled { get; set; }

        [DataMember(Name = "created")]
        public DateTime Created { get; set; }

        [DataMember(Name = "transaction")]
        public string Transaction { get; set; }

        [DataMember(Name = "error")]
        public string Error { get; set; }

        [DataMember(Name = "error_state")]
        public string ErrorState { get; set; } 

        [DataMember(Name = "processing")]
        public bool Processing { get; set; }

        [DataMember(Name = "refunded_amount")]
        public int RefundedAmount { get; set; }

        [DataMember(Name = "authorized_amount")]
        public int AuthorizedAmount { get; set; }

    }
}
