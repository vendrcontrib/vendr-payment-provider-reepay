using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vendr.Contrib.PaymentProviders.Reepay;
using Vendr.Core;
using Vendr.Core.Models;
using Vendr.Core.Web;
using Vendr.Core.Web.Api;
using Vendr.Core.Web.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders
{
    [PaymentProvider("reepay", "Reepay", "Reepay payment provider", Icon = "icon-invoice")]
    public class ReepayPaymentProvider : PaymentProviderBase<ReepaySettings>
    {
        public ReepayPaymentProvider(VendrContext vendr)
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

        public override OrderReference GetOrderReference(HttpRequestBase request, ReepaySettings settings)
        {
            try
            {
                var reepayEvent = GetWebhookReepayEvent(request);
                if (reepayEvent != null)
                {
                    if (!string.IsNullOrWhiteSpace(reepayEvent.EventId))
                    {
                        return OrderReference.Parse(reepayEvent.EventId);
                    }
                }
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<ReepayPaymentProvider>(ex, "Reepay - GetOrderReference");
            }

            return base.GetOrderReference(request, settings);
        }

        public override PaymentFormResult GenerateForm(OrderReadOnly order, string continueUrl, string cancelUrl, string callbackUrl, ReepaySettings settings)
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
                var basicAuth = Base64Encode(settings.PrivateKey + ":");

                var payment = $"https://checkout-api.reepay.com/v1/session/charge"
                            .WithHeader("Authorization", "Basic " + basicAuth)
                            .WithHeader("Content-Type", "application/json")
                            .PostJsonAsync(new
                            {
                                order = new
                                {
                                    handle = order.OrderNumber,
                                    amount = orderAmount,
                                    currency = currencyCode,
                                    customer = new
                                    {
                                        email = order.CustomerInfo.Email,
                                        handle = order.CustomerInfo.CustomerReference,
                                        first_name = order.CustomerInfo.FirstName,
                                        last_name = order.CustomerInfo.LastName
                                    }
                                },
                                locale = settings.Lang,
                                settle = false,
                                //payment_methods = paymentMethods != null && paymentMethods.Length > 0 ? "[" + string.Join(",", paymentMethods.Select(x => $"\"{x}\"")) + "]" : "[]",
                                accept_url = continueUrl,
                                cancel_url = cancelUrl
                            })
                            .ReceiveJson<ReepayChargeSessionDto>().Result;

                // Get charge session id
                chargeSessionId = payment.Id;

                paymentFormLink = payment.Url;
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<ReepayPaymentProvider>(ex, "Reepay - error creating payment.");
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

        public override string GetCancelUrl(OrderReadOnly order, ReepaySettings settings)
        {
            settings.MustNotBeNull("settings");
            settings.CancelUrl.MustNotBeNull("settings.CancelUrl");

            return settings.CancelUrl;
        }

        public override string GetErrorUrl(OrderReadOnly order, ReepaySettings settings)
        {
            settings.MustNotBeNull("settings");
            settings.ErrorUrl.MustNotBeNull("settings.ErrorUrl");

            return settings.ErrorUrl;
        }

        public override string GetContinueUrl(OrderReadOnly order, ReepaySettings settings)
        {
            settings.MustNotBeNull("settings");
            settings.ContinueUrl.MustNotBeNull("settings.ContinueUrl");

            return settings.ContinueUrl;
        }

        public override CallbackResult ProcessCallback(OrderReadOnly order, HttpRequestBase request, ReepaySettings settings)
        {
            try
            {
                // Process callback

                var reepayEvent = GetWebhookReepayEvent(request);

                // signature = hexencode(hmac_sha_256(webhook_secret, timestamp + id))

                return new CallbackResult
                {
                    TransactionInfo = new TransactionInfo
                    {
                        AmountAuthorized = order.TotalPrice.Value.WithTax,
                        TransactionFee = 0m,
                        TransactionId = Guid.NewGuid().ToString("N"),
                        PaymentStatus = PaymentStatus.Authorized
                    }
                };
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<ReepayPaymentProvider>(ex, "Reepay - ProcessCallback");
            }

            return CallbackResult.BadRequest();
        }

        public override ApiResult FetchPaymentStatus(OrderReadOnly order, ReepaySettings settings)
        {
            // Get charge: https://reference.reepay.com/api/#get-charge

            try
            {
                var basicAuth = Base64Encode(settings.PrivateKey + ":");
                var handle = order.OrderNumber;

                var payment = $"https://api.reepay.com/v1/charge/{handle}"
                            .WithHeader("Authorization", "Basic " + basicAuth)
                            .WithHeader("Content-Type", "application/json")
                            .GetJsonAsync<ReepayChargeDto>().Result;

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
                Vendr.Log.Error<ReepayPaymentProvider>(ex, "Reepay - FetchPaymentStatus");
            }

            return ApiResult.Empty;
        }

        public override ApiResult CancelPayment(OrderReadOnly order, ReepaySettings settings)
        {
            // Cancel charge: https://reference.reepay.com/api/#cancel-charge

            try
            {
                var basicAuth = Base64Encode(settings.PrivateKey + ":");
                var handle = order.OrderNumber;

                var payment = $"https://api.reepay.com/v1/charge/{handle}/cancel"
                            .WithHeader("Authorization", "Basic " + basicAuth)
                            .WithHeader("Content-Type", "application/json")
                            .PostAsync(null)
                            .ReceiveJson<ReepayChargeDto>().Result;

                return new ApiResult()
                {
                    TransactionInfo = new TransactionInfoUpdate()
                    {
                        TransactionId = order.TransactionInfo.TransactionId,
                        PaymentStatus = GetPaymentStatus(payment)
                    }
                };
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<ReepayPaymentProvider>(ex, "Reepay - CancelPayment");
            }

            return ApiResult.Empty;
        }

        public override ApiResult CapturePayment(OrderReadOnly order, ReepaySettings settings)
        {
            // Settle charge: https://reference.reepay.com/api/#settle-charge

            try
            {
                var basicAuth = Base64Encode(settings.PrivateKey + ":");
                var handle = order.OrderNumber;

                var payment = $"https://api.reepay.com/v1/charge/{handle}/settle"
                    .WithHeader("Authorization", "Basic " + basicAuth)
                    .WithHeader("Content-Type", "application/json")
                    .PostJsonAsync(new
                    {
                        amount = AmountToMinorUnits(order.TransactionInfo.AmountAuthorized.Value)
                    })
                    .ReceiveJson<ReepayChargeDto>().Result;
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<ReepayPaymentProvider>(ex, "Reepay - CapturePayment");
            }

            return ApiResult.Empty;
        }

        public override ApiResult RefundPayment(OrderReadOnly order, ReepaySettings settings)
        {
            // Create refund: https://reference.reepay.com/api/#create-refund

            try
            {
                var basicAuth = Base64Encode(settings.PrivateKey + ":");
                var handle = order.OrderNumber;

                var payment = $"https://api.reepay.com/v1/refund"
                    .WithHeader("Authorization", "Basic " + basicAuth)
                    .WithHeader("Content-Type", "application/json")
                    .PostJsonAsync(new
                    {
                        invoice = handle,
                        amount = AmountToMinorUnits(order.TransactionInfo.AmountAuthorized.Value)
                    })
                    .ReceiveJson<ReepayChargeDto>().Result;
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<ReepayPaymentProvider>(ex, "Reepay - RefundPayment");
            }

            return ApiResult.Empty;
        }

        protected PaymentStatus GetPaymentStatus(ReepayChargeDto payment)
        {
            // Possible Payment statuses:
            // - authorized
            // - settled
            // - failed
            // - cancelled
            // - pending

            if (payment.State == "authorized")
                return PaymentStatus.Authorized;

            if (payment.State == "settled")
                return PaymentStatus.Captured;

            if (payment.State == "failed")
                return PaymentStatus.Error;

            if (payment.State == "cancelled")
                return PaymentStatus.Cancelled;

            if (payment.State == "pending")
                return PaymentStatus.PendingExternalSystem;

            return PaymentStatus.Initialized;
        }

        protected string GetTransactionId(ReepayChargeDto payment)
        {
            return payment?.Transaction;
        }

        private ReepayWebhookEvent GetWebhookReepayEvent(HttpRequestBase request)
        {
            ReepayWebhookEvent reepayEvent = null;

            if (HttpContext.Current.Items["Vendr_ReepayEvent"] != null)
            {
                reepayEvent = (ReepayWebhookEvent)HttpContext.Current.Items["Vendr_ReepayEvent"];
            }
            else
            {
                try
                {
                    if (request.InputStream.CanSeek)
                        request.InputStream.Seek(0, SeekOrigin.Begin);

                    using (var reader = new StreamReader(request.InputStream))
                    {
                        var json = reader.ReadToEnd();

                        // Validate the webhook signature: https://reference.reepay.com/api/#webhooks


                        //reepayEvent = JsonConvert.DeserializeObject<ReepayWebhookEvent>(json);

                    }
                }
                catch (Exception ex)
                {
                    Vendr.Log.Error<ReepayPaymentProvider>(ex, "Reepay - GetWebhookReepayEvent");
                }
            }

            return reepayEvent;
        }
    }
}
