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

        public ReepaySessionChargeResult CreateSessionCharge(ReepaySessionCharge data)
        {
            return Request("/v1/session/charge", true, (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<ReepaySessionChargeResult>());
        }

        public ReepayCharge GetCharge(string handle)
        {
            return Request($"/v1/charge/{handle}", false, (req) => req
                .GetJsonAsync<ReepayCharge>());
        }

        public ReepayCharge CancelCharge(string handle)
        {
            return Request($"/v1/charge/{handle}/cancel", false, (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostAsync(null)
                .ReceiveJson<ReepayCharge>());
        }

        public ReepayCharge SettleCharge(string handle, object data)
        {
            return Request($"/v1/charge/{handle}/settle", false, (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<ReepayCharge>());
        }

        public ReepayCharge RefundCharge(object data)
        {
            return Request($"/v1/refund", false, (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<ReepayCharge>());
        }

        public Dictionary<string, object> GetInvoiceMetaData(string handle)
        {
            return Request($"/v1/invoice/{handle}/metadata", false, (req) => req
                .GetJsonAsync<Dictionary<string, object>>());
        }

        private TResult Request<TResult>(string url, bool checkoutApi, Func<IFlurlRequest, Task<TResult>> func)
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

                result = func.Invoke(req).Result;
            }
            catch (FlurlHttpException ex)
            {
                throw;
            }

            return result;
        }
    }
}
