using System.Runtime.Serialization;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    public enum WebhookEventType
    {
        /// <summary>
        /// Invoice has been created
        /// </summary>
        [EnumMember(Value = "invoice_created")]
        InvoiceCreated,

        /// <summary>
        /// Invoice has been settled
        /// </summary>
        [EnumMember(Value = "invoice_settled")]
        InvoiceSettled,

        /// <summary>
        /// Invoice has been authorized
        /// </summary>
        [EnumMember(Value = "invoice_authorized")]
        InvoiceAuthorized,

        /// <summary>
        /// Invoice has entered dunning state
        /// </summary>
        [EnumMember(Value = "invoice_dunning")]
        InvoiceDunning,

        /// <summary>
        /// Time for sending dunning notification acording to dunning plan
        /// </summary>
        [EnumMember(Value = "invoice_dunning_notification")]
        InvoiceDunningNotification,

        /// <summary>
        /// An ongoing dunning process has been cancelled either because the invoice has been settled or because the invoice has been cancelled
        /// </summary>
        [EnumMember(Value = "invoice_dunning_cancelled")]
        InvoiceDunningCancelled,

        /// <summary>
        /// Invoice has failed after unsuccessful dunning process or because of instant ondemand settle
        /// </summary>
        [EnumMember(Value = "invoice_failed")]
        InvoiceFailed,

        /// <summary>
        /// A refund has been performed on a settled invoice
        /// </summary>
        [EnumMember(Value = "invoice_refund")]
        InvoiceRefund,

        /// <summary>
        /// For asynchronous refund this event will be fired if the refund fails. An asynchronous refund will be indicated by the refund state "processing" when performing the refund.
        /// </summary>
        [EnumMember(Value = "invoice_refund_failed")]
        InvoiceRefundFailed,

        /// <summary>
        /// A failed or cancelled invoice has been reactivated to state pending for processing
        /// </summary>
        [EnumMember(Value = "invoice_reactivate")]
        InvoiceReactivate,

        /// <summary>
        /// Invoice has been cancelled
        /// </summary>
        [EnumMember(Value = "invoice_cancelled")]
        InvoiceCancelled,

        /// <summary>
        /// State has not changed but other attributes on the invoice has changed. E.g. settle later cancel.
        /// </summary>
        [EnumMember(Value = "invoice_changed")]
        InvoiceChanged,

        /// <summary>
        /// Invoice has been credited by creating attached an credit note and creating a subscription credit
        /// </summary>
        [EnumMember(Value = "invoice_credited")]
        InvoiceCredited,

        /// <summary>
        /// Subscription has been created
        /// </summary>
        [EnumMember(Value = "subscription_created")]
        SubscriptionCreated,

        /// <summary>
        /// A payment method has been added to the subscription for the first time
        /// </summary>
        [EnumMember(Value = "subscription_payment_method_added")]
        SubscriptionPaymentMethodAdded,

        /// <summary>
        /// The payment method has been changed for a subscription with an existing payment method
        /// </summary>
        [EnumMember(Value = "subscription_payment_method_changed")]
        SubscriptionPaymentMethodChanged,

        /// <summary>
        /// The trial period for subscription has ended
        /// </summary>
        [EnumMember(Value = "subscription_trial_end")]
        SubscriptionTrialEnd,

        /// <summary>
        /// An invoice has been made and new billing period has started for subscription
        /// </summary>
        [EnumMember(Value = "subscription_renewal")]
        SubscriptionRenewal,

        /// <summary>
        /// Subscription has been cancelled to expire at end of current billing period
        /// </summary>
        [EnumMember(Value = "subscription_cancelled")]
        SubscriptionCancelled,

        /// <summary>
        /// A previous cancellation has been cancelled
        /// </summary>
        [EnumMember(Value = "subscription_uncancelled")]
        SubscriptionUncancelled,

        /// <summary>
        /// Subscription has been put on hold by request
        /// </summary>
        [EnumMember(Value = "subscription_on_hold")]
        SubscriptionOnHold,

        /// <summary>
        /// Subscription has been put on hold due to a failed dunning process
        /// </summary>
        [EnumMember(Value = "subscription_on_hold_dunning")]
        SubscriptionOnHoldDunning,

        /// <summary>
        /// Subscription on hold has been reactivated to active state
        /// </summary>
        [EnumMember(Value = "subscription_reactivated")]
        SubscriptionReactivated,

        /// <summary>
        /// Subscription has expired either by request, end of fixed life time or because cancelled and billing period has ended
        /// </summary>
        [EnumMember(Value = "subscription_expired")]
        SubscriptionExpired,

        /// <summary>
        /// Subscription has expired due to a failed dunning process
        /// </summary>
        [EnumMember(Value = "subscription_expired_dunning")]
        SubscriptionExpiredDunning,

        /// <summary>
        /// Subscription scheduling or pricing has been changed, e.g. by changed plan or changed next period start
        /// </summary>
        [EnumMember(Value = "subscription_changed")]
        SubscriptionChanged,

        /// <summary>
        /// Customer has been created
        /// </summary>
        [EnumMember(Value = "customer_created")]
        CustomerCreated,

        /// <summary>
        /// A payment method has been added to customer
        /// </summary>
        [EnumMember(Value = "customer_payment_method_added")]
        CustomerPaymentMethodAdded,

        /// <summary>
        /// Customer information has been changed
        /// </summary>
        [EnumMember(Value = "customer_changed")]
        CustomerChanged,

        /// <summary>
        /// Customer has been deleted
        /// </summary>
        [EnumMember(Value = "customer_deleted")]
        CustomerDeleted
    }
}
