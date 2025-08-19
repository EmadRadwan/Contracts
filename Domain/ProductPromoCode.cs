using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class ProductPromoCode
{
    public ProductPromoCode()
    {
        OrderProductPromoCodes = new HashSet<OrderProductPromoCode>();
        ProdPromoCodeContactMeches = new HashSet<ProdPromoCodeContactMech>();
        ProductPromoCodeEmails = new HashSet<ProductPromoCodeEmail>();
        ProductPromoCodeParties = new HashSet<ProductPromoCodeParty>();
        ProductPromoUses = new HashSet<ProductPromoUse>();
        ShoppingLists = new HashSet<ShoppingList>();
    }

    public string ProductPromoCodeId { get; set; } = null!;
    public string? ProductPromoId { get; set; }
    public string? UserEntered { get; set; }
    public string? RequireEmailOrParty { get; set; }
    public int? UseLimitPerCode { get; set; }
    public int? UseLimitPerCustomer { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? CreatedByUserLoginNavigation { get; set; }
    public UserLogin? LastModifiedByUserLoginNavigation { get; set; }
    public ProductPromo? ProductPromo { get; set; }
    public ICollection<OrderProductPromoCode> OrderProductPromoCodes { get; set; }
    public ICollection<ProdPromoCodeContactMech> ProdPromoCodeContactMeches { get; set; }
    public ICollection<ProductPromoCodeEmail> ProductPromoCodeEmails { get; set; }
    public ICollection<ProductPromoCodeParty> ProductPromoCodeParties { get; set; }
    public ICollection<ProductPromoUse> ProductPromoUses { get; set; }
    public ICollection<ShoppingList> ShoppingLists { get; set; }
}