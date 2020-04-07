using Newtonsoft.Json;

namespace Vendr.Contrib.PaymentProviders.Reepay
{
    public class ReepayCustomerDto
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("address2")]
        public string Address2 { get; set; }

        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("vat")]
        public string VAT { get; set; }

        [JsonProperty("handle")]
        public string Handle { get; set; }

        [JsonProperty("generate_handle")]
        public bool GenerateHandle { get; set; }

        [JsonProperty("test")]
        public bool Test { get; set; }
    }
}
