using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
    public abstract class ReepayPaymentProviderBase<TSelf, TSettings> : PaymentProviderBase<TSettings>
        where TSelf : ReepayPaymentProviderBase<TSelf, TSettings>
        where TSettings : ReepaySettingsBase, new()
    {
        protected readonly ILogger<TSelf> _logger;

        public ReepayPaymentProviderBase(VendrContext vendr,
            ILogger<TSelf> logger)
            : base(vendr)
        {
            _logger = logger;
        }

        public override string GetCancelUrl(PaymentProviderContext<TSettings> ctx)
        {
            ctx.Settings.MustNotBeNull("settings");
            ctx.Settings.CancelUrl.MustNotBeNull("settings.CancelUrl");

            return ctx.Settings.CancelUrl;
        }

        public override string GetContinueUrl(PaymentProviderContext<TSettings> ctx)
        {
            ctx.Settings.MustNotBeNull("settings");
            ctx.Settings.ContinueUrl.MustNotBeNull("settings.ContinueUrl");

            return ctx.Settings.ContinueUrl;
        }

        public override string GetErrorUrl(PaymentProviderContext<TSettings> ctx)
        {
            ctx.Settings.MustNotBeNull("settings");
            ctx.Settings.ErrorUrl.MustNotBeNull("settings.ErrorUrl");

            return ctx.Settings.ErrorUrl;
        }

        public override async Task<OrderReference> GetOrderReferenceAsync(PaymentProviderContext<TSettings> ctx)
        {
            try
            {
                var reepayEvent = await GetReepayWebhookEventAsync(ctx);
                if (reepayEvent != null)
                {
                    if (!string.IsNullOrWhiteSpace(reepayEvent.Invoice) &&
                        (reepayEvent.EventType == WebhookEventType.InvoiceAuthorized ||
                         reepayEvent.EventType == WebhookEventType.InvoiceSettled))
                    {
                        var clientConfig = GetReepayClientConfig(ctx.Settings);
                        var client = new ReepayClient(clientConfig);
                        var metadata = await client.GetInvoiceMetaData(reepayEvent.Invoice);
                        if (metadata != null)
                        {
                            if (metadata.TryGetValue("orderReference", out object orderReference))
                            {
                                return OrderReference.Parse(orderReference.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Reepay - GetOrderReference");
            }

            return await base.GetOrderReferenceAsync(ctx);
        }

        protected PaymentStatus GetPaymentStatus(ReepayCharge charge)
        {
            // Possible Charge statuses:
            // - authorized
            // - settled
            // - failed
            // - cancelled
            // - pending

            if (charge.State == "authorized")
                return PaymentStatus.Authorized;

            if (charge.State == "settled")
            {
                if (charge.RefundedAmount > 0)
                    return PaymentStatus.Refunded;

                return PaymentStatus.Captured;
            }

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
            // Possible Refund statuses:
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

        protected async Task<ReepayWebhookEvent> GetReepayWebhookEventAsync(PaymentProviderContext<TSettings> ctx)
        {
            ReepayWebhookEvent reepayEvent = null;

            if (ctx.AdditionalData.ContainsKey("Vendr_ReepayEvent"))
            {
                reepayEvent = (ReepayWebhookEvent)ctx.AdditionalData["Vendr_ReepayEvent"];
            }
            else
            {
                try
                {
                    using (var stream = await ctx.Request.Content.ReadAsStreamAsync())
                    {
                        if (stream.CanSeek)
                            stream.Seek(0, SeekOrigin.Begin);

                        using (var sr = new StreamReader(stream))
                        using (var jr = new JsonTextReader(sr) { DateParseHandling = DateParseHandling.None })
                        {
                            while (jr.Read())
                            {
                                JObject obj = (JObject)JToken.ReadFrom(jr);

                                if (obj != null)
                                {
                                    if (obj.TryGetValue("signature", out JToken signature) &&
                                        obj.TryGetValue("timestamp", out JToken timestamp) &&
                                        obj.TryGetValue("id", out JToken id))
                                    {
                                        // Validate the webhook signature: https://reference.reepay.com/api/#webhooks
                                        var calcSignature = CalculateSignature(ctx.Settings.WebhookSecret, timestamp.Value<string>(), id.Value<string>());

                                        if (signature.Value<string>() == calcSignature)
                                        {
                                            var json = obj.ToString(Formatting.None);

                                            reepayEvent = JsonConvert.DeserializeObject<ReepayWebhookEvent>(json);

                                            ctx.AdditionalData.Add("Vendr_ReepayEvent", reepayEvent);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Reepay - GetReepayWebhookEvent");
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
