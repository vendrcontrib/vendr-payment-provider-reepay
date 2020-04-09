using Vendr.Contrib.PaymentProviders.Reepay;
using Vendr.Core.Web.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders
{
    public class ReepayCheckoutOneTimeSettings : ReepaySettingsBase
    {
        [PaymentProviderSetting(Name = "Auto Capture",
            Description = "Flag indicating whether to immediately capture the payment, or whether to just authorize the payment for later (manual) capture.",
            SortOrder = 1000)]
        public bool Capture { get; set; }
    }
}
