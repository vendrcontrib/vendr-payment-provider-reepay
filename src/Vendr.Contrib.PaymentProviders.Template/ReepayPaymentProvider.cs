using Flurl.Http;
using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vendr.Core;
using Vendr.Core.Models;
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

            // https://docs.reepay.com/docs/new-web-shop

            try
            {
                var basicAuth = Base64Encode(settings.PrivateKey + ":");

                var payment = $"https://checkout-api.reepay.com/v1/session/charge"
                            .WithHeader("Authorization", "Basic " + basicAuth)
                            .WithHeader("Content-Type", "application/json")
                            .PostJsonAsync(new
                            {
                                order = new {
                                    handle = order.OrderNumber,
                                    amount = orderAmount,
                                    currency = currencyCode,
                                    customer = new {
                                        email = order.CustomerInfo.Email,
                                        handle = order.CustomerInfo.CustomerReference,
                                        first_name = order.CustomerInfo.FirstName,
                                        last_name = order.CustomerInfo.LastName
                                    }
                                },
                                accept_url = continueUrl,
                                cancel_url = cancelUrl
                            })
                            .ReceiveJson().Result;
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<ReepayPaymentProvider>(ex, "Reepay - error creating payment.");
            }

            return new PaymentFormResult()
            {
                Form = new PaymentForm(continueUrl, FormMethod.Post)
            };
        }

        public override string GetCancelUrl(OrderReadOnly order, ReepaySettings settings)
        {
            return string.Empty;
        }

        public override string GetErrorUrl(OrderReadOnly order, ReepaySettings settings)
        {
            return string.Empty;
        }

        public override string GetContinueUrl(OrderReadOnly order, ReepaySettings settings)
        {
            settings.MustNotBeNull("settings");
            settings.ContinueUrl.MustNotBeNull("settings.ContinueUrl");

            return settings.ContinueUrl;
        }

        public override CallbackResult ProcessCallback(OrderReadOnly order, HttpRequestBase request, ReepaySettings settings)
        {
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
    }
}
