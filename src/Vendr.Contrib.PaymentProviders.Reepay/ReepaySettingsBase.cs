using Vendr.Core.Web.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders.Reepay
{
    public class ReepaySettingsBase
    {
        [PaymentProviderSetting(Name = "Continue URL",
            Description = "The URL to continue to after this provider has done processing. eg: /continue/",
            SortOrder = 100)]
        public string ContinueUrl { get; set; }

        [PaymentProviderSetting(Name = "Cancel URL",
            Description = "The URL to return to if the payment attempt is canceled. eg: /cancel/",
            SortOrder = 200)]
        public string CancelUrl { get; set; }

        [PaymentProviderSetting(Name = "Error URL",
            Description = "The URL to return to if the payment attempt errors. eg: /error/",
            SortOrder = 300)]
        public string ErrorUrl { get; set; }

        [PaymentProviderSetting(Name = "Private Key",
            Description = "Private Key from the Reepay administration portal.",
            SortOrder = 500)]
        public string PrivateKey { get; set; }

        [PaymentProviderSetting(Name = "Webhook Secret",
            Description = "Webhook Secret from the Reepay administration portal.",
            SortOrder = 600)]
        public string WebhookSecret { get; set; }

        [PaymentProviderSetting(Name = "Language",
            Description = "The language of the payment portal to display.",
            SortOrder = 800)]
        public string Lang { get; set; }

        [PaymentProviderSetting(Name = "Accepted Payment Methods",
            Description = "A comma separated list of Payment Methods to accept.",
            SortOrder = 900)]
        public string PaymentMethods { get; set; }
    }
}
