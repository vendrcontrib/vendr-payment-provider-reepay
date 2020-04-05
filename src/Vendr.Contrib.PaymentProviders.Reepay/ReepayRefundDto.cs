using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Vendr.Contrib.PaymentProviders.Reepay
{
    [DataContract]
    public class ReepayRefundDto
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "state")]
        public string State { get; set; }

        [DataMember(Name = "invoice")]
        public string Invoice { get; set; }

        [DataMember(Name = "amount")]
        public int Amount { get; set; }

        [DataMember(Name = "currency")]
        public string Currency { get; set; }

        [DataMember(Name = "transaction")]
        public string Transaction { get; set; }

        [DataMember(Name = "error")]
        public string Error { get; set; }

        [DataMember(Name = "error_state")]
        public string ErrorState { get; set; }

        [DataMember(Name = "acquirer_message")]
        public string AcquirerMessage { get; set; }

        [DataMember(Name = "created")]
        public DateTime Created { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }
    }
}
