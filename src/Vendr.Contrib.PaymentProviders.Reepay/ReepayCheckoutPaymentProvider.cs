using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vendr.Contrib.PaymentProviders.Reepay.Api;
using Vendr.Contrib.PaymentProviders.Reepay.Api.Models;
using Vendr.Core;
using Vendr.Core.Models;
using Vendr.Core.Web;
using Vendr.Core.Web.Api;
using Vendr.Core.Web.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders.Reepay
{
    [PaymentProvider("reepay-checkout", "Reepay Checkout", "Reepay payment provider for one time payments")]
    public class ReepayCheckoutPaymentProvider : ReepayPaymentProviderBase<ReepayCheckoutSettings>
    {
        public ReepayCheckoutPaymentProvider(VendrContext vendr)
            : base(vendr)
        { }

        public override bool CanCancelPayments => true;
        public override bool CanCapturePayments => true;
        public override bool CanRefundPayments => true;
        public override bool CanFetchPaymentStatus => true;

        // We'll finalize via webhook callback
        public override bool FinalizeAtContinueUrl => false;

        public override IEnumerable<TransactionMetaDataDefinition> TransactionMetaDataDefinitions => new[]{
            new TransactionMetaDataDefinition("reepaySessionId", "Reepay Session ID"),
            new TransactionMetaDataDefinition("reepaySubscriptionId", "Reepay Subscription ID"),
            new TransactionMetaDataDefinition("reepayCustomerHandle", "Reepay Customer Handle")
        };

        public override PaymentFormResult GenerateForm(OrderReadOnly order, string continueUrl, string cancelUrl, string callbackUrl, ReepayCheckoutSettings settings)
        {
            var currency = Vendr.Services.CurrencyService.GetCurrency(order.CurrencyId);
            var currencyCode = currency.Code.ToUpperInvariant();

            // Ensure currency has valid ISO 4217 code
            if (!Iso4217.CurrencyCodes.ContainsKey(currencyCode))
            {
                throw new Exception("Currency must be a valid ISO 4217 currency code: " + currency.Name);
            }

            var billingCountry = order.PaymentInfo.CountryId.HasValue
                ? Vendr.Services.CountryService.GetCountry(order.PaymentInfo.CountryId.Value)
                : null;

            var paymentMethods = settings.PaymentMethods?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Where(x => !string.IsNullOrWhiteSpace(x))
                   .Select(s => s.Trim())
                   .ToArray();

            var customerHandle = !string.IsNullOrEmpty(order.CustomerInfo.CustomerReference)
                                        ? order.CustomerInfo.CustomerReference
                                        : Guid.NewGuid().ToString();

            string paymentFormLink = string.Empty;

            var sessionId = order.Properties["reepaySessionId"]?.Value;

            // https://docs.reepay.com/docs/new-web-shop

            try
            {
                var metaData = new Dictionary<string, string>()
                {
                    { "orderReference", order.GenerateOrderReference() }
                };

                var hasRecurringItems = false;
                long recurringTotalPrice = 0;
                long orderTotalPrice = AmountToMinorUnits(order.TotalPrice.Value.WithTax);

                foreach (var orderLine in order.OrderLines.Where(IsRecurringOrderLine))
                {
                    var orderLinePrice = AmountToMinorUnits(orderLine.TotalPrice.Value.WithTax);

                    hasRecurringItems = true;
                }

                if (recurringTotalPrice < orderTotalPrice)
                {

                }

                var checkoutSessionRequest = new ReepayChargeSessionRequest
                {
                    Order = new ReepayOrder
                    {
                        Key = order.GenerateOrderReference(),
                        Handle = order.OrderNumber,
                        Amount = (int)orderTotalPrice,
                        Currency = currencyCode,
                        Customer = new ReepayCustomer
                        {
                            Email = order.CustomerInfo.Email,
                            Handle = customerHandle,
                            FirstName = order.CustomerInfo.FirstName,
                            LastName = order.CustomerInfo.LastName,
                            Company = !string.IsNullOrWhiteSpace(settings.BillingCompanyPropertyAlias)
                                ? order.Properties[settings.BillingCompanyPropertyAlias] : null,
                            Address = !string.IsNullOrWhiteSpace(settings.BillingAddressLine1PropertyAlias)
                                ? order.Properties[settings.BillingAddressLine1PropertyAlias] : null,
                            Address2 = !string.IsNullOrWhiteSpace(settings.BillingAddressLine2PropertyAlias)
                                ? order.Properties[settings.BillingAddressLine2PropertyAlias] : null,
                            PostalCode = !string.IsNullOrWhiteSpace(settings.BillingAddressZipCodePropertyAlias)
                                ? order.Properties[settings.BillingAddressZipCodePropertyAlias] : null,
                            City = !string.IsNullOrWhiteSpace(settings.BillingAddressCityPropertyAlias)
                                ? order.Properties[settings.BillingAddressCityPropertyAlias] : null,
                            Phone = !string.IsNullOrWhiteSpace(settings.BillingPhonePropertyAlias)
                                ? order.Properties[settings.BillingPhonePropertyAlias] : null,
                            Country = billingCountry?.Code,
                            GenerateHandle = string.IsNullOrEmpty(customerHandle)
                        },
                        BillingAddress = new ReepayAddress
                        {
                            FirstName = order.CustomerInfo.FirstName,
                            LastName = order.CustomerInfo.LastName,
                            Company = !string.IsNullOrWhiteSpace(settings.BillingCompanyPropertyAlias)
                                ? order.Properties[settings.BillingCompanyPropertyAlias] : null,
                            Address = !string.IsNullOrWhiteSpace(settings.BillingAddressLine1PropertyAlias)
                                ? order.Properties[settings.BillingAddressLine1PropertyAlias] : null,
                            Address2 = !string.IsNullOrWhiteSpace(settings.BillingAddressLine2PropertyAlias)
                                ? order.Properties[settings.BillingAddressLine2PropertyAlias] : null,
                            PostalCode = !string.IsNullOrWhiteSpace(settings.BillingAddressZipCodePropertyAlias)
                                ? order.Properties[settings.BillingAddressZipCodePropertyAlias] : null,
                            City = !string.IsNullOrWhiteSpace(settings.BillingAddressCityPropertyAlias)
                                ? order.Properties[settings.BillingAddressCityPropertyAlias] : null,
                            Phone = !string.IsNullOrWhiteSpace(settings.BillingPhonePropertyAlias)
                                ? order.Properties[settings.BillingPhonePropertyAlias] : null,
                            Country = billingCountry?.Code
                        },
                        MetaData = metaData
                    },
                    Recurring = hasRecurringItems,
                    Settle = settings.Capture,
                    AcceptUrl = continueUrl,
                    CancelUrl = cancelUrl
                };

                if (!string.IsNullOrWhiteSpace(settings.Locale))
                {
                    checkoutSessionRequest.Locale = settings.Locale;
                }

                if (paymentMethods?.Length > 0)
                {
                    // Set payment methods if any exists otherwise omit.
                    checkoutSessionRequest.PaymentMethods = paymentMethods;
                }

                //if (hasRecurringItems)
                //{
                //    var rsr = new ReepayRecurringSessionRequest
                //    {
                //        Customer = order.CustomerInfo.CustomerReference,
                //        CreateCustomer = new ReepayCustomer
                //        {
                //            Email = order.CustomerInfo.Email,
                //            Handle = customerHandle,
                //            FirstName = order.CustomerInfo.FirstName,
                //            LastName = order.CustomerInfo.LastName,
                //            Company = !string.IsNullOrWhiteSpace(settings.BillingCompanyPropertyAlias)
                //                ? order.Properties[settings.BillingCompanyPropertyAlias] : null,
                //            Address = !string.IsNullOrWhiteSpace(settings.BillingAddressLine1PropertyAlias)
                //                ? order.Properties[settings.BillingAddressLine1PropertyAlias] : null,
                //            Address2 = !string.IsNullOrWhiteSpace(settings.BillingAddressLine2PropertyAlias)
                //                ? order.Properties[settings.BillingAddressLine2PropertyAlias] : null,
                //            PostalCode = !string.IsNullOrWhiteSpace(settings.BillingAddressZipCodePropertyAlias)
                //                ? order.Properties[settings.BillingAddressZipCodePropertyAlias] : null,
                //            City = !string.IsNullOrWhiteSpace(settings.BillingAddressCityPropertyAlias)
                //                ? order.Properties[settings.BillingAddressCityPropertyAlias] : null,
                //            Phone = !string.IsNullOrWhiteSpace(settings.BillingPhonePropertyAlias)
                //                ? order.Properties[settings.BillingPhonePropertyAlias] : null,
                //            Country = billingCountry?.Code,
                //            GenerateHandle = string.IsNullOrEmpty(customerHandle)
                //        },
                //        AcceptUrl = continueUrl,
                //        CancelUrl = cancelUrl
                //    };

                //    if (!string.IsNullOrWhiteSpace(settings.Locale))
                //    {
                //        rsr.Locale = settings.Locale;
                //    }

                //    if (paymentMethods?.Length > 0)
                //    {
                //        // Set payment methods if any exists otherwise omit.
                //        rsr.PaymentMethods = paymentMethods;
                //    }
                //}

                var clientConfig = GetReepayClientConfig(settings);
                var client = new ReepayClient(clientConfig);

                // Create checkout session
                var checkoutSession = client.CreateChargeSession(checkoutSessionRequest);
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
                Vendr.Log.Error<ReepayCheckoutPaymentProvider>(ex, "Reepay - error creating payment.");
            }

            return new PaymentFormResult()
            {
                MetaData = new Dictionary<string, string>
                {
                    { "reepaySessionId", sessionId },
                    { "reepayCustomerHandle", customerHandle }
                },
                Form = new PaymentForm(paymentFormLink, FormMethod.Get)
                            .WithJsFile("https://checkout.reepay.com/checkout.js")
                            .WithJs(@"var rp = new Reepay.WindowCheckout('" + sessionId + "');")
            };
        }

        public override CallbackResult ProcessCallback(OrderReadOnly order, HttpRequestBase request, ReepayCheckoutSettings settings)
        {
            try
            {
                // Process callback

                var reepayEvent = GetReepayWebhookEvent(request, settings);
                if (reepayEvent != null)
                {
                    if (reepayEvent.EventType == WebhookEventType.InvoiceAuthorized || reepayEvent.EventType == WebhookEventType.InvoiceSettled)
                    {
                        var transactionMetaData = new Dictionary<string, string>();

                        var hasRecurringItems = order.OrderLines.Any(IsRecurringOrderLine);
                        if (hasRecurringItems)
                        {
                            var clientConfig = GetReepayClientConfig(settings);
                            var client = new ReepayClient(clientConfig);

                            var charge = client.GetCharge(order.OrderNumber);
                            if (charge != null)
                            {
                                var metaData = new Dictionary<string, string>()
                                {
                                    { "orderReference", order.GenerateOrderReference() }
                                };

                                var orderLine = order.OrderLines.FirstOrDefault(x => IsRecurringOrderLine(x) && x.Properties.ContainsKey("reepayPlanHandle") && !string.IsNullOrWhiteSpace(x.Properties["reepayPlanHandle"]));
                                if (orderLine != null)
                                {
                                    var data = new ReepaySubscriptionRequest
                                    {
                                        Handle = $"S-{order.OrderNumber}", //"s-101", // Get plan from order line property?
                                        Plan = orderLine.Properties["reepayPlanHandle"],
                                        SignupMethod = SignupMethod.Source,
                                        Customer = reepayEvent.Customer, //order.CustomerInfo.CustomerReference,
                                        Source = charge.RecurringPaymentMethod, //reepayEvent.PaymentMethod,
                                        GenerateHandle = false,
                                        MetaData = metaData,
                                        StartDate = orderLine.Properties["reepayStartDate"],
                                        EndDate = orderLine.Properties["reepayEndDate"]
                                    };

                                    try
                                    {
                                        // Create subscription
                                        var subscription = client.CreateSubscription(data);
                                        if (subscription != null)
                                        {
                                            transactionMetaData.Add("reepaySubscriptionHandle", subscription.Handle);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Vendr.Log.Error<ReepayCheckoutPaymentProvider>(ex, "Reepay - Error creating subscription");
                                    }
                                }
                            }
                        }

                        return CallbackResult.Ok(new TransactionInfo
                        {
                            TransactionId = reepayEvent.Transaction,
                            AmountAuthorized = order.TotalPrice.Value.WithTax,
                            PaymentStatus = reepayEvent.EventType == WebhookEventType.InvoiceSettled ? PaymentStatus.Captured : PaymentStatus.Authorized
                        }, transactionMetaData);
                    }

                    if (reepayEvent.EventType == WebhookEventType.SubscriptionCreated)
                    {
                        // Subscription created

                    }
                }
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<ReepayCheckoutPaymentProvider>(ex, "Reepay - ProcessCallback");
            }

            return CallbackResult.BadRequest();
        }

        public override ApiResult FetchPaymentStatus(OrderReadOnly order, ReepayCheckoutSettings settings)
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
                Vendr.Log.Error<ReepayCheckoutPaymentProvider>(ex, "Reepay - FetchPaymentStatus");
            }

            return ApiResult.Empty;
        }

        public override ApiResult CancelPayment(OrderReadOnly order, ReepayCheckoutSettings settings)
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
                Vendr.Log.Error<ReepayCheckoutPaymentProvider>(ex, "Reepay - CancelPayment");
            }

            return ApiResult.Empty;
        }

        public override ApiResult CapturePayment(OrderReadOnly order, ReepayCheckoutSettings settings)
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
                Vendr.Log.Error<ReepayCheckoutPaymentProvider>(ex, "Reepay - CapturePayment");
            }

            return ApiResult.Empty;
        }

        public override ApiResult RefundPayment(OrderReadOnly order, ReepayCheckoutSettings settings)
        {
            // Create refund: https://reference.reepay.com/api/#create-refund

            try
            {
                var clientConfig = GetReepayClientConfig(settings);
                var client = new ReepayClient(clientConfig);

                var data = new
                {
                    invoice = order.OrderNumber,
                    amount = AmountToMinorUnits(order.TransactionInfo.AmountAuthorized.Value)
                };

                // Refund charge
                var refund = client.RefundCharge(data);

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
                Vendr.Log.Error<ReepayCheckoutPaymentProvider>(ex, "Reepay - RefundPayment");
            }

            return ApiResult.Empty;
        }

        private bool IsRecurringOrderLine(OrderLineReadOnly orderLine)
        {
            return orderLine.Properties.ContainsKey(Constants.Properties.Product.IsRecurringPropertyAlias)
                && !string.IsNullOrWhiteSpace(orderLine.Properties[Constants.Properties.Product.IsRecurringPropertyAlias])
                && (orderLine.Properties[Constants.Properties.Product.IsRecurringPropertyAlias] == "1"
                    || orderLine.Properties[Constants.Properties.Product.IsRecurringPropertyAlias].Value.Equals("true", StringComparison.OrdinalIgnoreCase));
        }
    }
}
