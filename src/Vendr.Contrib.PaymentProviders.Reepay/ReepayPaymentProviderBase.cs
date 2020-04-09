using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Vendr.Contrib.PaymentProviders.Reepay.Api.Models;
using Vendr.Core;
using Vendr.Core.Models;
using Vendr.Core.Web.Api;
using Vendr.Core.Web.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders.Reepay
{
    public abstract class ReepayPaymentProviderBase<TSettings> : PaymentProviderBase<TSettings>
        where TSettings : ReepaySettingsBase, new()
    {
        public ReepayPaymentProviderBase(VendrContext vendr)
            : base(vendr)
        { }

        public override string GetCancelUrl(OrderReadOnly order, TSettings settings)
        {
            settings.MustNotBeNull("settings");
            settings.CancelUrl.MustNotBeNull("settings.CancelUrl");

            return settings.CancelUrl;
        }

        public override string GetContinueUrl(OrderReadOnly order, TSettings settings)
        {
            settings.MustNotBeNull("settings");
            settings.ContinueUrl.MustNotBeNull("settings.ContinueUrl");

            return settings.ContinueUrl;
        }

        public override string GetErrorUrl(OrderReadOnly order, TSettings settings)
        {
            settings.MustNotBeNull("settings");
            settings.ErrorUrl.MustNotBeNull("settings.ErrorUrl");

            return settings.ErrorUrl;
        }

        protected PaymentStatus GetPaymentStatus(ReepayCharge payment)
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

        protected string GetTransactionId(ReepayCharge payment)
        {
            return payment?.Transaction;
        }

        protected ReepayClientConfig GetReepayClientConfig(ReepaySettingsBase settings)
        {
            var basicAuth = Base64Encode(settings.PrivateKey + ":");

            return new ReepayClientConfig
            {
                BaseUrl = "https://api.reepay.com",
                Authorization = "Basic " + basicAuth
            };
        }

        protected ReepayWebhookEvent GetReepayWebhookEvent(HttpRequestBase request, ReepaySettingsBase settings)
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

                        if (!string.IsNullOrEmpty(json) && JObject.Parse(json).TryGetValue("signature", out JToken token))
                        {
                            // Validate the webhook signature: https://reference.reepay.com/api/#webhooks
                            //var signature = CalculateSignature(settings.WebhookSecret, timestamp + id);

                            //if (token.Value<string>() == signature)
                            //{
                            reepayEvent = JsonConvert.DeserializeObject<ReepayWebhookEvent>(json);
                            //}
                        }
                    }
                }
                catch (Exception ex)
                {
                    Vendr.Log.Error<ReepayPaymentProviderBase<TSettings>>(ex, "Reepay - GetReepayWebhookEvent");
                }
            }

            return reepayEvent;
        }

        private string CalculateSignature(string webhookSecret, string timestamp, string id)
        {
            // signature = hexencode(hmac_sha_256(webhook_secret, timestamp + id))

            var signature = ComputeSignature(webhookSecret, timestamp, id);

            return signature;
        }

        private string ComputeSignature(string secret, string timestamp, string id)
        {
            using (var cryptographer = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(timestamp + id);
                var hash = cryptographer.ComputeHash(buffer);
                return HexEncode(hash).ToLowerInvariant();
            }
        }

        private string HexEncode(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", string.Empty);
        }
    }
}
