using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class PaymentMethodType
{
    public PaymentMethodType()
    {
        OrderPaymentPreferences = new HashSet<OrderPaymentPreference>();
        PaymentGatewayResponses = new HashSet<PaymentGatewayResponse>();
        PaymentMethodTypeGlAccounts = new HashSet<PaymentMethodTypeGlAccount>();
        PaymentMethods = new HashSet<PaymentMethod>();
        Payments = new HashSet<Payment>();
        ProductPaymentMethodTypes = new HashSet<ProductPaymentMethodType>();
        ProductStorePaymentSettings = new HashSet<ProductStorePaymentSetting>();
        ProductStoreVendorPayments = new HashSet<ProductStoreVendorPayment>();
    }

    public string PaymentMethodTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }

    public string? DefaultGlAccountId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlAccount? DefaultGlAccount { get; set; }
    public ICollection<OrderPaymentPreference> OrderPaymentPreferences { get; set; }
    public ICollection<PaymentGatewayResponse> PaymentGatewayResponses { get; set; }
    public ICollection<PaymentMethodTypeGlAccount> PaymentMethodTypeGlAccounts { get; set; }
    public ICollection<PaymentMethod> PaymentMethods { get; set; }
    public ICollection<Payment> Payments { get; set; }
    public ICollection<ProductPaymentMethodType> ProductPaymentMethodTypes { get; set; }
    public ICollection<ProductStorePaymentSetting> ProductStorePaymentSettings { get; set; }
    public ICollection<ProductStoreVendorPayment> ProductStoreVendorPayments { get; set; }
}