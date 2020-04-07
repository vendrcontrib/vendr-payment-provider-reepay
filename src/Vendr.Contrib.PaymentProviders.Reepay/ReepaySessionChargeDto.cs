using System.Runtime.Serialization;

namespace Vendr.Contrib.PaymentProviders.Reepay
{
    [DataContract]
    public class ReepaySessionChargeDto
    {
        [DataMember(Name = "configuration", EmitDefaultValue = false)]
        public string Configuration { get; set; }

        [DataMember(Name = "locale", EmitDefaultValue = false)]
        public string Locale { get; set; }

        [DataMember(Name = "settle")]
        public bool Settle { get; set; }

        [DataMember(Name = "order")]
        public ReepayOrderDto Order { get; set; }

        [DataMember(Name = "recurring")]
        public bool Recurring { get; set; }

        [DataMember(Name = "accept_url")]
        public string AcceptUrl { get; set; }

        [DataMember(Name = "cancel_url")]
        public string CancelUrl { get; set; }

        [DataMember(Name = "payment_methods", EmitDefaultValue = false)]
        public string[] PaymentMethods { get; set; }
    }
}
