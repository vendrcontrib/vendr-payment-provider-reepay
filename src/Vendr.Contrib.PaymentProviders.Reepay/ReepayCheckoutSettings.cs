﻿using Vendr.Core.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders.Reepay
{
    public class ReepayCheckoutSettings : ReepaySettingsBase
    {
        [PaymentProviderSetting(Name = "Auto Capture",
            Description = "Flag indicating whether to immediately capture the payment, or whether to just authorize the payment for later (manual) capture.",
            SortOrder = 1500)]
        public bool Capture { get; set; }

        [PaymentProviderSetting(Name = "Button Text",
            Description = "Optional alternative button text. Maximum length 32 characters.",
            SortOrder = 1600)]
        public string ButtonText { get; set; }
        
    }
}
