using System.Runtime.Serialization;

namespace Vendr.Contrib.PaymentProviders.Reepay
{
    [DataContract]
    public class ReepayOrderDto
    {
        [DataMember(Name = "handle")]
        public string Handle { get; set; }

        [DataMember(Name = "key")]
        public string Key { get; set; }

        [DataMember(Name = "amount")]
        public int Amount { get; set; }

        [DataMember(Name = "currency")]
        public string Currency { get; set; }

        [DataMember(Name = "customer")]
        public ReepayCustomerDto Customer { get; set; }
    }
}
