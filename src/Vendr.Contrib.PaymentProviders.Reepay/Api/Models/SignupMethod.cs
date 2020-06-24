using System.Runtime.Serialization;

namespace Vendr.Contrib.PaymentProviders.Reepay.Api.Models
{
    public enum SignupMethod
    {
        [EnumMember(Value = "source")]
        Source,

        [EnumMember(Value = "email")]
        Email,

        [EnumMember(Value = "link")]
        Link,
    }
}
