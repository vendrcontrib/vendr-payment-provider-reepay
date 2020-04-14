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

        protected PaymentStatus GetPaymentStatus(ReepayCharge charge)
        {
            // Possible Payment statuses:
            // - authorized
            // - settled
            // - failed
            // - cancelled
            // - pending

            if (charge.State == "authorized")
                return PaymentStatus.Authorized;

            if (charge.State == "settled")
                return PaymentStatus.Captured;

            if (charge.State == "failed")
                return PaymentStatus.Error;

            if (charge.State == "cancelled")
                return PaymentStatus.Cancelled;

            if (charge.State == "pending")
                return PaymentStatus.PendingExternalSystem;

            return PaymentStatus.Initialized;
        }

        protected PaymentStatus GetPaymentStatus(ReepayRefund refund)
        {
            // Possible Payment statuses:
            // - refunded
            // - failed
            // - processing

            if (refund.State == "refunded")
                return PaymentStatus.Refunded;

            if (refund.State == "failed")
                return PaymentStatus.Error;

            if (refund.State == "processing")
                return PaymentStatus.PendingExternalSystem;

            return PaymentStatus.Authorized;
        }

        protected string GetTransactionId(ReepayCharge charge)
        {
            return charge?.Transaction;
        }

        protected string GetTransactionId(ReepayRefund refund)
        {
            return refund?.Transaction;
        }

        protected ReepayClientConfig GetReepayClientConfig(ReepaySettingsBase settings)
        {
            var basicAuth = Base64Encode(settings.PrivateKey + ":");

            return new ReepayClientConfig
            {
                BaseUrl = "https://api.reepay.com",
                Authorization = "Basic " + basicAuth,
                WebhookSecret = settings.WebhookSecret
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

                    using (var sr = new StreamReader(request.InputStream))
                    using (var jr = new JsonTextReader(sr) { DateParseHandling = DateParseHandling.None })
                    {
                        while(jr.Read())
                        {
                            JObject obj = (JObject)JToken.ReadFrom(jr);

                            if (obj != null)
                            {
                                if (obj.TryGetValue("signature", out JToken signature) &&
                                    obj.TryGetValue("timestamp", out JToken timestamp) &&
                                    obj.TryGetValue("id", out JToken id))
                                {
                                    // Validate the webhook signature: https://reference.reepay.com/api/#webhooks
                                    var calcSignature = CalculateSignature(settings.WebhookSecret, timestamp.Value<string>(), id.Value<string>());

                                    if (signature.Value<string>() == calcSignature)
                                    {
                                        var json = obj.ToString(Formatting.None);

                                        reepayEvent = JsonConvert.DeserializeObject<ReepayWebhookEvent>(json);
                                    }
                                }
                            }
                        }

                        HttpContext.Current.Items["Vendr_ReepayEvent"] = reepayEvent;
                    }
                }
                catch (Exception ex)
                {
                    Vendr.Log.Error<ReepayPaymentProviderBase<TSettings>>(ex, "Reepay - GetReepayWebhookEvent");
                }
            }

            return reepayEvent;
        }

        protected string GenerateReference(string input)
        {
            var hash = MD5Hash(input);
            return new Guid(hash).ToString();
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
