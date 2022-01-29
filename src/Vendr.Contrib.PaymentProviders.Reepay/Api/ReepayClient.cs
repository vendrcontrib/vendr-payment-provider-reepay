using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vendr.Contrib.PaymentProviders.Reepay.Api.Models;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api
{
    public class ReepayClient
    {
        private ReepayClientConfig _config;

        public ReepayClient(ReepayClientConfig config)
        {
            _config = config;
        }

        public async Task<ReepaySessionResponse> CreateChargeSessionAsync(ReepayChargeSessionRequest data)
        {
            return await Request("/v1/session/charge", true, (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<ReepaySessionResponse>());
        }

        public async Task<ReepaySessionResponse> CreateRecurringSessionAsync(ReepayChargeSessionRequest data)
        {
            return await Request("/v1/session/recurring", true, (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<ReepaySessionResponse>());
        }

        public async Task<ReepayCharge> GetChargeAsync(string handle)
        {
            return await Request($"/v1/charge/{handle}", false, (req) => req
                .GetJsonAsync<ReepayCharge>());
        }

        public async Task<ReepayCharge> CancelChargeAsync(string handle)
        {
            return await Request($"/v1/charge/{handle}/cancel", false, (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostAsync(null)
                .ReceiveJson<ReepayCharge>());
        }

        public async Task<ReepayCharge> SettleChargeAsync(string handle, object data)
        {
            return await Request($"/v1/charge/{handle}/settle", false, (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<ReepayCharge>());
        }

        public async Task<ReepayRefund> RefundChargeAsync(object data)
        {
            return await Request($"/v1/refund", false, (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<ReepayRefund>());
        }

        public async Task<ReepaySubscription> CreateSubscriptionAsync(object data)
        {
            return await Request($"/v1/subscription", false, (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<ReepaySubscription>());
        }

        public async Task<ReepaySubscription> GetSubscriptionAsync(string handle)
        {
            return await Request($"/v1/subscription/{handle}", false, (req) => req
                .WithHeader("Content-Type", "application/json")
                .GetJsonAsync<ReepaySubscription>());
        }

        public async Task<ReepaySubscription> CancelSubscriptionAsync(string handle, object data)
        {
            return await Request($"/v1/subscription/{handle}/cancel", false, (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<ReepaySubscription>());
        }

        public async Task<ReepaySubscription> UncancelSubscription(string handle)
        {
            return await Request($"/v1/subscription/{handle}/uncancel", false, (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(null)
                .ReceiveJson<ReepaySubscription>());
        }

        public async Task<ReepayInvoice> GetInvoice(string handle)
        {
            return await Request($"/v1/invoice/{handle}", false, (req) => req
                .GetJsonAsync<ReepayInvoice>());
        }

        public async Task<Dictionary<string, object>> GetInvoiceMetaData(string handle)
        {
            return await Request($"/v1/invoice/{handle}/metadata", false, (req) => req
                .GetJsonAsync<Dictionary<string, object>>());
        }

        private async Task<TResult> Request<TResult>(string url, bool checkoutApi, Func<IFlurlRequest, Task<TResult>> func)
        {
            var result = default(TResult);

            try
            {
                var baseUrl = checkoutApi ? _config.BaseUrl.Replace("api.", "checkout-api.") : _config.BaseUrl;

                var req = new FlurlRequest(baseUrl + url)
                        .ConfigureRequest(x =>
                        {
                            var jsonSettings = new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                DefaultValueHandling = DefaultValueHandling.Include,
                                MissingMemberHandling = MissingMemberHandling.Ignore
                            };
                            x.JsonSerializer = new NewtonsoftJsonSerializer(jsonSettings);
                        })
                        .WithHeader("Authorization", _config.Authorization);

                result = await func.Invoke(req);
            }
            catch (FlurlHttpException ex)
            {
                throw;
            }

            return result;
        }
    }
}
