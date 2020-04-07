using System.Runtime.Serialization;

namespace Vendr.Contrib.PaymentProviders.Reepay
{
    [DataContract]
    public class ReepayCustomerDto
    {
        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "first_name")]
        public string FirstName { get; set; }

        [DataMember(Name = "last_name")]
        public string LastName { get; set; }

        [DataMember(Name = "address")]
        public string Address { get; set; }

        [DataMember(Name = "address2")]
        public string Address2 { get; set; }

        [DataMember(Name = "postal_code")]
        public string PostalCode { get; set; }

        [DataMember(Name = "city")]
        public string City { get; set; }

        [DataMember(Name = "country")]
        public string Country { get; set; }

        [DataMember(Name = "phone")]
        public string Phone { get; set; }

        [DataMember(Name = "company")]
        public string Company { get; set; }

        [DataMember(Name = "vat")]
        public string VAT { get; set; }

        [DataMember(Name = "handle")]
        public string Handle { get; set; }

        [DataMember(Name = "test")]
        public bool Test { get; set; }
    }
}
