﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vendr.Contrib.PaymentProviders.Reepay;
using Vendr.Contrib.PaymentProviders.Reepay.Api;
using Vendr.Contrib.PaymentProviders.Reepay.Api.Models;
using Vendr.Core;
using Vendr.Core.Models;
using Vendr.Core.Web.Api;
using Vendr.Core.Web.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders
{
    [PaymentProvider("reepay-checkout-onetime", "Reepay (One Time)", "Reepay payment provider for one time payments")]
    public class ReepayCheckoutOneTimePaymentProvider : ReepayPaymentProviderBase<ReepayCheckoutOneTimeSettings>
    {
        public ReepayCheckoutOneTimePaymentProvider(VendrContext vendr)
            : base(vendr)
        { }

        public override bool CanCancelPayments => true;
        public override bool CanCapturePayments => true;
        public override bool CanRefundPayments => true;
        public override bool CanFetchPaymentStatus => true;

        public override bool FinalizeAtContinueUrl => true;

        public override IEnumerable<TransactionMetaDataDefinition> TransactionMetaDataDefinitions => new[]{
            new TransactionMetaDataDefinition("reepayChargeSessionId", "Reepay Charge Session ID")
        };

        public override OrderReference GetOrderReference(HttpRequestBase request, ReepayCheckoutOneTimeSettings settings)
        {
            try
            {
                var reepayEvent = GetReepayWebhookEvent(request, settings);
                if (reepayEvent != null)
                {
                    if (!string.IsNullOrWhiteSpace(reepayEvent.EventId) && 
                        (reepayEvent.EventType == "invoice_authorized" || reepayEvent.EventType == "invoice_settled"))
                    {
                        return OrderReference.Parse(reepayEvent.EventId);
                    }
                }
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<ReepayCheckoutOneTimePaymentProvider>(ex, "Reepay - GetOrderReference");
            }

            return base.GetOrderReference(request, settings);
        }

        public override PaymentFormResult GenerateForm(OrderReadOnly order, string continueUrl, string cancelUrl, string callbackUrl, ReepayCheckoutOneTimeSettings settings)
        {
            var currency = Vendr.Services.CurrencyService.GetCurrency(order.CurrencyId);
            var currencyCode = currency.Code.ToUpperInvariant();

            // Ensure currency has valid ISO 4217 code
            if (!Iso4217.CurrencyCodes.ContainsKey(currencyCode))
            {
                throw new Exception("Currency must a valid ISO 4217 currency code: " + currency.Name);
            }

            var orderAmount = AmountToMinorUnits(order.TotalPrice.Value.WithTax).ToString("0", CultureInfo.InvariantCulture);

            var paymentMethods = settings.PaymentMethods?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Where(x => !string.IsNullOrWhiteSpace(x))
                   .Select(s => s.Trim())
                   .ToArray();

            string paymentFormLink = string.Empty;

            var chargeSessionId = order.Properties["reepayChargeSessionId"]?.Value;

            // https://docs.reepay.com/docs/new-web-shop

            try
            {
                var data = new ReepaySessionCharge
                {
                    Order = new ReepayOrder
                    {
                        Handle = order.OrderNumber,
                        Amount = Convert.ToInt32(orderAmount),
                        Currency = currencyCode,
                        Customer = new ReepayCustomer
                        {
                            Email = order.CustomerInfo.Email,
                            Handle = order.CustomerInfo.CustomerReference,
                            FirstName = order.CustomerInfo.FirstName,
                            LastName = order.CustomerInfo.LastName,
                            GenerateHandle = string.IsNullOrEmpty(order.CustomerInfo.CustomerReference)
                        }
                    },
                    Locale = settings.Lang,
                    Settle = settings.Capture,
                    AcceptUrl = continueUrl,
                    CancelUrl = cancelUrl
                };

                if (paymentMethods?.Length > 0)
                {
                    // Set payment methods if any exists otherwise omit.
                    data.PaymentMethods = paymentMethods;
                }

                var clientConfig = GetReepayClientConfig(settings);
                var client = new ReepayClient(clientConfig);

                // Create session charge
                var payment = client.CreateSessionCharge(data);

                // Get charge session id
                chargeSessionId = payment.Id;

                paymentFormLink = payment.Url;
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<ReepayCheckoutOneTimePaymentProvider>(ex, "Reepay - error creating payment.");
            }

            return new PaymentFormResult()
            {
                MetaData = new Dictionary<string, string>
                {
                    { "reepayChargeSessionId", chargeSessionId }
                },
                Form = new PaymentForm(paymentFormLink, FormMethod.Get)
                            .WithJsFile("https://checkout.reepay.com/checkout.js")
                            .WithJs(@"var rp = new Reepay.WindowCheckout('" + chargeSessionId + "');")
            };
        }

        public override CallbackResult ProcessCallback(OrderReadOnly order, HttpRequestBase request, ReepayCheckoutOneTimeSettings settings)
        {
            try
            {
                // Process callback

                var reepayEvent = GetReepayWebhookEvent(request, settings);

                if (reepayEvent != null && !string.IsNullOrWhiteSpace(reepayEvent.EventId) &&
                   (reepayEvent.EventType == "invoice_authorized" || reepayEvent.EventType == "invoice_settled"))
                {
                    return CallbackResult.Ok(new TransactionInfo
                    {
                        TransactionId = reepayEvent.Transaction,
                        AmountAuthorized = order.TotalPrice.Value.WithTax,
                        PaymentStatus = reepayEvent.EventType == "invoice_settled" ? PaymentStatus.Captured : PaymentStatus.Authorized
                    });
                    //, new Dictionary<string, string>
                    //{
                    //   { "reepayWebhookId", reepayEvent.Id }
                    //});
                }
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<ReepayCheckoutOneTimePaymentProvider>(ex, "Reepay - ProcessCallback");
            }

            return CallbackResult.BadRequest();
        }

        public override ApiResult FetchPaymentStatus(OrderReadOnly order, ReepayCheckoutOneTimeSettings settings)
        {
            // Get charge: https://reference.reepay.com/api/#get-charge

            try
            {
                var clientConfig = GetReepayClientConfig(settings);
                var client = new ReepayClient(clientConfig);

                // Get charge
                var payment = client.GetCharge(order.OrderNumber);

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
                Vendr.Log.Error<ReepayCheckoutOneTimePaymentProvider>(ex, "Reepay - FetchPaymentStatus");
            }

            return ApiResult.Empty;
        }

        public override ApiResult CancelPayment(OrderReadOnly order, ReepayCheckoutOneTimeSettings settings)
        {
            // Cancel charge: https://reference.reepay.com/api/#cancel-charge

            try
            {
                var clientConfig = GetReepayClientConfig(settings);
                var client = new ReepayClient(clientConfig);

                // Cancel charge
                var payment = client.CancelCharge(order.OrderNumber);

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
                Vendr.Log.Error<ReepayCheckoutOneTimePaymentProvider>(ex, "Reepay - CancelPayment");
            }

            return ApiResult.Empty;
        }

        public override ApiResult CapturePayment(OrderReadOnly order, ReepayCheckoutOneTimeSettings settings)
        {
            // Settle charge: https://reference.reepay.com/api/#settle-charge

            try
            {
                var clientConfig = GetReepayClientConfig(settings);
                var client = new ReepayClient(clientConfig);

                var data = new
                {
                    amount = AmountToMinorUnits(order.TransactionInfo.AmountAuthorized.Value)
                };

                // Settle charge
                var payment = client.SettleCharge(order.OrderNumber, data);

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
                Vendr.Log.Error<ReepayCheckoutOneTimePaymentProvider>(ex, "Reepay - CapturePayment");
            }

            return ApiResult.Empty;
        }

        public override ApiResult RefundPayment(OrderReadOnly order, ReepayCheckoutOneTimeSettings settings)
        {
            // Create refund: https://reference.reepay.com/api/#create-refund

            try
            {
                var clientConfig = GetReepayClientConfig(settings);
                var client = new ReepayClient(clientConfig);

                var data = new
                {
                    amount = AmountToMinorUnits(order.TransactionInfo.AmountAuthorized.Value)
                };

                // Refund charge
                var payment = client.RefundCharge(data);

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
                Vendr.Log.Error<ReepayCheckoutOneTimePaymentProvider>(ex, "Reepay - RefundPayment");
            }

            return ApiResult.Empty;
        }
    }
}