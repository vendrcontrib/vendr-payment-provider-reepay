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

        [DataMember(Name = "address", EmitDefaultValue = false)]
        public string Address { get; set; }

        [DataMember(Name = "address2", EmitDefaultValue = false)]
        public string Address2 { get; set; }

        [DataMember(Name = "postal_code", EmitDefaultValue = false)]
        public string PostalCode { get; set; }

        [DataMember(Name = "city", EmitDefaultValue = false)]
        public string City { get; set; }

        [DataMember(Name = "country", EmitDefaultValue = false)]
        public string Country { get; set; }

        [DataMember(Name = "phone", EmitDefaultValue = false)]
        public string Phone { get; set; }

        [DataMember(Name = "company", EmitDefaultValue = false)]
        public string Company { get; set; }

        [DataMember(Name = "vat", EmitDefaultValue = false)]
        public string VAT { get; set; }

        [DataMember(Name = "handle")]
        public string Handle { get; set; }

        [DataMember(Name = "handle")]
        public bool GenerateHandle { get; set; }

        [DataMember(Name = "test")]
        public bool Test { get; set; }
    }
}
