using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using System;
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
                .PostJsonAsync(data)
                .ReceiveJson<ReepaySessionChargeResult>());
        }

        private TResult Request<TResult>(string url, bool checkoutApi, Func<IFlurlRequest, Task<TResult>> func)
        {
            var result = default(TResult);

            try
            {
                //var basicAuth = Base64Encode(settings.PrivateKey + ":");

                var baseUrl = checkoutApi ? _config.BaseUrl.Replace("api.", "checkout-api.") : _config.BaseUrl;

                var req = new FlurlRequest(baseUrl + url)
                        .ConfigureRequest(x =>
                        {
                            var jsonSettings = new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            };
                            x.JsonSerializer = new NewtonsoftJsonSerializer(jsonSettings);
                        })
                        .WithHeader("Authorization", _config.Authorization)
                        .WithHeader("Content-Type", "application/json");

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
