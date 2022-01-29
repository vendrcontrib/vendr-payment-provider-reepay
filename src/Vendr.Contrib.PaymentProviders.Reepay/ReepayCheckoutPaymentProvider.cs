using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vendr.Common.Logging;
using Vendr.Contrib.PaymentProviders.Reepay.Api;
using Vendr.Contrib.PaymentProviders.Reepay.Api.Models;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Core.PaymentProviders;
using Vendr.Extensions;

namespace Vendr.Contrib.PaymentProviders.Reepay
{
    [PaymentProvider("reepay-checkout", "Reepay Checkout", "Reepay payment provider for one time payments")]
    public class ReepayCheckoutPaymentProvider : ReepayPaymentProviderBase<ReepayCheckoutPaymentProvider, ReepayCheckoutSettings>
    {
        public ReepayCheckoutPaymentProvider(VendrContext vendr, ILogger<ReepayCheckoutPaymentProvider> logger)
            : base(vendr, logger)
        { }

        public override bool CanCancelPayments => true;
        public override bool CanCapturePayments => true;
        public override bool CanRefundPayments => true;
        public override bool CanFetchPaymentStatus => true;

        // We'll finalize via webhook callback
        public override bool FinalizeAtContinueUrl => false;

        public override IEnumerable<TransactionMetaDataDefinition> TransactionMetaDataDefinitions => new[]{
            new TransactionMetaDataDefinition("reepaySessionId", "Reepay Session ID"),
            new TransactionMetaDataDefinition("reepayCustomerHandle", "Reepay Customer Handle")
        };

        public override async Task<PaymentFormResult> GenerateFormAsync(PaymentProviderContext<ReepayCheckoutSettings> ctx)
        {
            var currency = Vendr.Services.CurrencyService.GetCurrency(ctx.Order.CurrencyId);
            var currencyCode = currency.Code.ToUpperInvariant();

            // Ensure currency has valid ISO 4217 code
            if (!Iso4217.CurrencyCodes.ContainsKey(currencyCode))
            {
                throw new Exception("Currency must be a valid ISO 4217 currency code: " + currency.Name);
            }

            var billingCountry = ctx.Order.PaymentInfo.CountryId.HasValue
                ? Vendr.Services.CountryService.GetCountry(ctx.Order.PaymentInfo.CountryId.Value)
                : null;

            var orderAmount = AmountToMinorUnits(ctx.Order.TransactionAmount.Value);

            var paymentMethods = ctx.Settings.PaymentMethods?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Where(x => !string.IsNullOrWhiteSpace(x))
                   .Select(s => s.Trim())
                   .ToArray();

            var customerHandle = !string.IsNullOrEmpty(ctx.Order.CustomerInfo.CustomerReference)
                                        ? ctx.Order.CustomerInfo.CustomerReference
                                        : Guid.NewGuid().ToString();

            string paymentFormLink = string.Empty;

            var sessionId = ctx.Order.Properties["reepaySessionId"]?.Value;

            // https://docs.reepay.com/docs/new-web-shop

            try
            {
                var metaData = new Dictionary<string, string>()
                {
                    { "orderReference", ctx.Order.GenerateOrderReference() },
                    { "orderId", ctx.Order.Id.ToString("D") },
                    { "orderNumber", ctx.Order.OrderNumber }
                };

                var checkoutSessionRequest = new ReepayChargeSessionRequest
                {
                    Order = new ReepayOrder
                    {
                        Key = ctx.Order.GenerateOrderReference(),
                        Handle = ctx.Order.OrderNumber,
                        Amount = (int)orderAmount,
                        Currency = currencyCode,
                        Customer = new ReepayCustomer
                        {
                            Email = ctx.Order.CustomerInfo.Email,
                            Handle = customerHandle,
                            FirstName = ctx.Order.CustomerInfo.FirstName,
                            LastName = ctx.Order.CustomerInfo.LastName,
                            Company = !string.IsNullOrWhiteSpace(ctx.Settings.BillingCompanyPropertyAlias)
                                ? ctx.Order.Properties[ctx.Settings.BillingCompanyPropertyAlias] : null,
                            Address = !string.IsNullOrWhiteSpace(ctx.Settings.BillingAddressLine1PropertyAlias)
                                ? ctx.Order.Properties[ctx.Settings.BillingAddressLine1PropertyAlias] : null,
                            Address2 = !string.IsNullOrWhiteSpace(ctx.Settings.BillingAddressLine2PropertyAlias)
                                ? ctx.Order.Properties[ctx.Settings.BillingAddressLine2PropertyAlias] : null,
                            PostalCode = !string.IsNullOrWhiteSpace(ctx.Settings.BillingAddressZipCodePropertyAlias)
                                ? ctx.Order.Properties[ctx.Settings.BillingAddressZipCodePropertyAlias] : null,
                            City = !string.IsNullOrWhiteSpace(ctx.Settings.BillingAddressCityPropertyAlias)
                                ? ctx.Order.Properties[ctx.Settings.BillingAddressCityPropertyAlias] : null,
                            Phone = !string.IsNullOrWhiteSpace(ctx.Settings.BillingPhonePropertyAlias)
                                ? ctx.Order.Properties[ctx.Settings.BillingPhonePropertyAlias] : null,
                            Country = billingCountry?.Code,
                            GenerateHandle = string.IsNullOrEmpty(customerHandle)
                        },
                        BillingAddress = new ReepayAddress
                        {
                            FirstName = ctx.Order.CustomerInfo.FirstName,
                            LastName = ctx.Order.CustomerInfo.LastName,
                            Company = !string.IsNullOrWhiteSpace(ctx.Settings.BillingCompanyPropertyAlias)
                                ? ctx.Order.Properties[ctx.Settings.BillingCompanyPropertyAlias] : null,
                            Address = !string.IsNullOrWhiteSpace(ctx.Settings.BillingAddressLine1PropertyAlias)
                                ? ctx.Order.Properties[ctx.Settings.BillingAddressLine1PropertyAlias] : null,
                            Address2 = !string.IsNullOrWhiteSpace(ctx.Settings.BillingAddressLine2PropertyAlias)
                                ? ctx.Order.Properties[ctx.Settings.BillingAddressLine2PropertyAlias] : null,
                            PostalCode = !string.IsNullOrWhiteSpace(ctx.Settings.BillingAddressZipCodePropertyAlias)
                                ? ctx.Order.Properties[ctx.Settings.BillingAddressZipCodePropertyAlias] : null,
                            City = !string.IsNullOrWhiteSpace(ctx.Settings.BillingAddressCityPropertyAlias)
                                ? ctx.Order.Properties[ctx.Settings.BillingAddressCityPropertyAlias] : null,
                            Phone = !string.IsNullOrWhiteSpace(ctx.Settings.BillingPhonePropertyAlias)
                                ? ctx.Order.Properties[ctx.Settings.BillingPhonePropertyAlias] : null,
                            Country = billingCountry?.Code
                        },
                        MetaData = metaData
                    },
                    Settle = ctx.Settings.Capture,
                    AcceptUrl = ctx.Urls.ContinueUrl,
                    CancelUrl = ctx.Urls.CancelUrl
                };

                if (!string.IsNullOrWhiteSpace(ctx.Settings.Locale))
                {
                    checkoutSessionRequest.Locale = ctx.Settings.Locale;
                }

                if (paymentMethods?.Length > 0)
                {
                    // Set payment methods if any exists otherwise omit.
                    checkoutSessionRequest.PaymentMethods = paymentMethods;
                }

                var clientConfig = GetReepayClientConfig(ctx.Settings);
                var client = new ReepayClient(clientConfig);

                // Create checkout session
                var checkoutSession = await client.CreateChargeSessionAsync(checkoutSessionRequest);
                if (checkoutSession != null)
                {
                    // Get session id
                    sessionId = checkoutSession.Id;

                    // Get session url
                    paymentFormLink = checkoutSession.Url;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Reepay - error creating payment.");
            }

            return new PaymentFormResult()
            {
                MetaData = new Dictionary<string, string>
                {
                    { "reepaySessionId", sessionId },
                    { "reepayCustomerHandle", customerHandle }
                },
                Form = new PaymentForm(paymentFormLink, PaymentFormMethod.Get)
                            .WithJsFile("https://checkout.reepay.com/checkout.js")
                            .WithJs(@"var rp = new Reepay.WindowCheckout('" + sessionId + "');")
            };
        }

        public override async Task<CallbackResult> ProcessCallbackAsync(PaymentProviderContext<ReepayCheckoutSettings> ctx)
        {
            try
            {
                // Process callback

                var clientConfig = GetReepayClientConfig(ctx.Settings);
                var client = new ReepayClient(clientConfig);

                var reepayEvent = await GetReepayWebhookEventAsync(ctx);

                if (reepayEvent != null && (
                        reepayEvent.EventType == WebhookEventType.InvoiceAuthorized || 
                        reepayEvent.EventType == WebhookEventType.InvoiceSettled ||
                        reepayEvent.EventType == WebhookEventType.InvoiceRefund ||
                        reepayEvent.EventType == WebhookEventType.InvoiceCancelled
                    ))
                {
                    // Get invoice
                    var invoice = await client.GetInvoice(reepayEvent.Invoice);
                    if (invoice != null)
                    {
                        return CallbackResult.Ok(new TransactionInfo
                        {
                            TransactionId = reepayEvent.Transaction,
                            AmountAuthorized = ctx.Order.TransactionAmount.Value,
                            PaymentStatus = GetPaymentStatus(invoice)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Reepay - ProcessCallback");
            }

            return CallbackResult.BadRequest();
        }

        public override async Task<ApiResult> FetchPaymentStatusAsync(PaymentProviderContext<ReepayCheckoutSettings> ctx)
        {
            // Get charge: https://reference.reepay.com/api/#get-charge

            try
            {
                var clientConfig = GetReepayClientConfig(ctx.Settings);
                var client = new ReepayClient(clientConfig);

                // Get charge
                var payment = await client.GetChargeAsync(ctx.Order.OrderNumber);

                return new ApiResult()
                {
                    TransactionInfo = new TransactionInfoUpdate()
                    {
                        TransactionId = GetTransactionId(payment),
                        PaymentStatus = GetPaymentStatus(payment)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Reepay - FetchPaymentStatus");
            }

            return ApiResult.Empty;
        }

        public override async Task<ApiResult> CancelPaymentAsync(PaymentProviderContext<ReepayCheckoutSettings> ctx)
        {
            // Cancel charge: https://reference.reepay.com/api/#cancel-charge

            try
            {
                var clientConfig = GetReepayClientConfig(ctx.Settings);
                var client = new ReepayClient(clientConfig);

                // Cancel charge
                var payment = await client.CancelChargeAsync(ctx.Order.OrderNumber);

                return new ApiResult()
                {
                    TransactionInfo = new TransactionInfoUpdate()
                    {
                        TransactionId = GetTransactionId(payment),
                        PaymentStatus = GetPaymentStatus(payment)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Reepay - CancelPayment");
            }

            return ApiResult.Empty;
        }

        public override async Task<ApiResult> CapturePaymentAsync(PaymentProviderContext<ReepayCheckoutSettings> ctx)
        {
            // Settle charge: https://reference.reepay.com/api/#settle-charge

            try
            {
                var clientConfig = GetReepayClientConfig(ctx.Settings);
                var client = new ReepayClient(clientConfig);

                var data = new
                {
                    amount = AmountToMinorUnits(ctx.Order.TransactionInfo.AmountAuthorized.Value)
                };

                // Settle charge
                var payment = await client.SettleChargeAsync(ctx.Order.OrderNumber, data);

                return new ApiResult()
                {
                    TransactionInfo = new TransactionInfoUpdate()
                    {
                        TransactionId = GetTransactionId(payment),
                        PaymentStatus = GetPaymentStatus(payment)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Reepay - CapturePayment");
            }

            return ApiResult.Empty;
        }

        public override async Task<ApiResult> RefundPaymentAsync(PaymentProviderContext<ReepayCheckoutSettings> ctx)
        {
            // Create refund: https://reference.reepay.com/api/#create-refund

            try
            {
                var clientConfig = GetReepayClientConfig(ctx.Settings);
                var client = new ReepayClient(clientConfig);

                var data = new
                {
                    invoice = ctx.Order.OrderNumber,
                    amount = AmountToMinorUnits(ctx.Order.TransactionInfo.AmountAuthorized.Value)
                };

                // Refund charge
                var refund = await client.RefundChargeAsync(data);

                return new ApiResult()
                {
                    TransactionInfo = new TransactionInfoUpdate()
                    {
                        TransactionId = GetTransactionId(refund),
                        PaymentStatus = GetPaymentStatus(refund)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Reepay - RefundPayment");
            }

            return ApiResult.Empty;
        }
    }
}
