using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class PaymentMethod
{
    public PaymentMethod()
    {
        FinAccounts = new HashSet<FinAccount>();
        OrderPaymentPreferences = new HashSet<OrderPaymentPreference>();
        PartyAcctgPreferences = new HashSet<PartyAcctgPreference>();
        PaymentGatewayResponses = new HashSet<PaymentGatewayResponse>();
        Payments = new HashSet<Payment>();
        ReturnHeaders = new HashSet<ReturnHeader>();
        ShoppingLists = new HashSet<ShoppingList>();
    }

    public string PaymentMethodId { get; set; } = null!;
    public string? PaymentMethodTypeId { get; set; }
    public string? PartyId { get; set; }
    public string? GlAccountId { get; set; }
    public string? FinAccountId { get; set; }
    public string? Description { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FinAccount? FinAccount { get; set; }
    public GlAccount? GlAccount { get; set; }
    public Party? Party { get; set; }
    public PaymentMethodType? PaymentMethodType { get; set; }
    public CheckAccount CheckAccount { get; set; } = null!;
    public CreditCard CreditCard { get; set; } = null!;
    public EftAccount EftAccount { get; set; } = null!;
    public GiftCard GiftCard { get; set; } = null!;
    public PayPalPaymentMethod PayPalPaymentMethod { get; set; } = null!;
    public ICollection<FinAccount> FinAccounts { get; set; }
    public ICollection<OrderPaymentPreference> OrderPaymentPreferences { get; set; }
    public ICollection<PartyAcctgPreference> PartyAcctgPreferences { get; set; }
    public ICollection<PaymentGatewayResponse> PaymentGatewayResponses { get; set; }
    public ICollection<Payment> Payments { get; set; }
    public ICollection<ReturnHeader> ReturnHeaders { get; set; }
    public ICollection<ShoppingList> ShoppingLists { get; set; }
}