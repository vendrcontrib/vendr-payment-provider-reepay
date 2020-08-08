using Newtonsoft.Json;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    public class ReepaySource
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("card")]
        public string Card { get; set; }

        [JsonProperty("mps")]
        public string MPS { get; set; }

        [JsonProperty("fingerprint")]
        public string Fingerprint { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("auth_transaction")]
        public string AuthTransaction { get; set; }

        [JsonProperty("card_type")]
        public string CardType { get; set; }

        [JsonProperty("exp_date")]
        public string ExpDate { get; set; }

        [JsonProperty("masked_card")]
        public string MaskedCard { get; set; }

        [JsonProperty("strong_authentication_status")]
        public string StrongAuthenticationStatus { get; set; }

        [JsonProperty("three_d_secure_status")]
        public string ThreeDSecureStatus { get; set; }

        [JsonProperty("risk_rule")]
        public string RiskRule { get; set; }

        [JsonProperty("acquirer_code")]
        public string AcquirerCode { get; set; }

        [JsonProperty("acquirer_message")]
        public string AcquirerMessage { get; set; }

        [JsonProperty("acquirer_reference")]
        public string AcquirerReference { get; set; }

        [JsonProperty("text_on_statement")]
        public string TextOnStatement { get; set; }

        [JsonProperty("surcharge_fee")]
        public int SurchargeFee { get; set; }
    }
}