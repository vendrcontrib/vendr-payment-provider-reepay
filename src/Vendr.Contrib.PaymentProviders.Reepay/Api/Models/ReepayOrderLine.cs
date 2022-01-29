using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    /// <summary>
    /// Reepay orderline object: https://reference.reepay.com/api/#orderline
    /// </summary>
    public class ReepayOrderLine
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("ordertext")]
        public string OrderText { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("vat")]
        public decimal VAT { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("origin")]
        public string Origin { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("discounted_amount")]
        public int? DiscountedAmount { get; set; }

        [JsonProperty("amount_vat")]
        public int? AmountVAT { get; set; }

        [JsonProperty("amount_ex_vat")]
        public int AmountExVAT { get; set; }

        [JsonProperty("unit_amount")]
        public int UnitAmount { get; set; }

        [JsonProperty("unit_amount_vat")]
        public int UnitAmountVAT { get; set; }

        [JsonProperty("unit_amount_ex_vat")]
        public int UnitAmountExVAT { get; set; }

        [JsonProperty("amount_defined_incl_vat")]
        public bool AmountDefinedInclVAT { get; set; }

        [JsonProperty("origin_handle")]
        public string OriginHandle { get; set; }

        [JsonProperty("period_from")]
        public DateTime? PeriodFrom { get; set; }

        [JsonProperty("period_to")]
        public DateTime? PeriodTo { get; set; }

        [JsonProperty("discount_percentage")]
        public int? DiscountPercentage { get; set; }

        [JsonProperty("discounted_order_line")]
        public string DiscountedOrderLine { get; set; }
    }
}
