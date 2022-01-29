using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    /// <summary>
    /// Reepay invoice object: https://reference.reepay.com/api/#invoice
    /// </summary>
    public class ReepayInvoice
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("handle")]
        public string Handle { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("customer")]
        public string Customer { get; set; }

        [JsonProperty("subscription")]
        public string Subscription { get; set; }

        [JsonProperty("plan")]
        public string Plan { get; set; }

        [JsonProperty("plan_version")]
        public string PlanVersion { get; set; }

        [JsonProperty("processing")]
        public bool? Processing { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("number")]
        public int? Number { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("due")]
        public DateTime? Due { get; set; }

        [JsonProperty("authorized")]
        public DateTime? Authorized { get; set; }

        [JsonProperty("settled")]
        public DateTime? Settled { get; set; }

        [JsonProperty("cancelled")]
        public DateTime? Cancelled { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("order_lines")]
        public List<ReepayOrderLine> OrderLines { get; set; }

        [JsonProperty("transactions")]
        public List<ReepayTransaction> Transactions { get; set; }

        [JsonProperty("discount_amount")]
        public int DiscountAmount { get; set; }

        [JsonProperty("org_amount")]
        public int OrgAmount { get; set; }

        [JsonProperty("amount_vat")]
        public int AmountVAT { get; set; }

        [JsonProperty("amount_ex_vat")]
        public int AmountExVAT { get; set; }

        [JsonProperty("settled_amount")]
        public int SettledAmount { get; set; }

        [JsonProperty("refunded_amount")]
        public int RefundedAmount { get; set; }

        [JsonProperty("authorized_amount")]
        public int? AuthorizedAmount { get; set; }

        [JsonProperty("credited_amount")]
        public int? CreditedAmount { get; set; }

        [JsonProperty("period_number")]
        public int? PeriodNumber { get; set; }

        [JsonProperty("recurring_payment_method")]
        public string RecurringPaymentMethod { get; set; }

        [JsonProperty("settle_later_payment_method")]
        public string SettleLaterPaymentMethod { get; set; }

        [JsonProperty("settle_later")]
        public bool? SettleLater { get; set; }

        [JsonProperty("period_from")]
        public DateTime? PeriodFrom { get; set; }

        [JsonProperty("period_to")]
        public DateTime? PeriodTo { get; set; }

        [JsonProperty("dunning_plan")]
        public string DunningPlan { get; set; }

        [JsonProperty("dunning_start")]
        public DateTime? DunningStart { get; set; }

        [JsonProperty("dunning_count")]
        public int? DunningCount { get; set; }

        [JsonProperty("dunning_expired")]
        public string DunningExpired { get; set; }

        [JsonProperty("billing_address")]
        public ReepayAddress BillingAddress { get; set; }

        [JsonProperty("shipping_address")]
        public ReepayAddress ShippingAddress { get; set; }
    }
}
