using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class FinAccountType
{
    public FinAccountType()
    {
        FinAccountTypeAttrs = new HashSet<FinAccountTypeAttr>();
        FinAccountTypeGlAccounts = new HashSet<FinAccountTypeGlAccount>();
        FinAccounts = new HashSet<FinAccount>();
        InverseParentType = new HashSet<FinAccountType>();
        ProductStoreFinActSettings = new HashSet<ProductStoreFinActSetting>();
    }

    public string FinAccountTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? ReplenishEnumId { get; set; }
    public string? IsRefundable { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FinAccountType? ParentType { get; set; }
    public Enumeration? ReplenishEnum { get; set; }
    public ICollection<FinAccountTypeAttr> FinAccountTypeAttrs { get; set; }
    public ICollection<FinAccountTypeGlAccount> FinAccountTypeGlAccounts { get; set; }
    public ICollection<FinAccount> FinAccounts { get; set; }
    public ICollection<FinAccountType> InverseParentType { get; set; }
    public ICollection<ProductStoreFinActSetting> ProductStoreFinActSettings { get; set; }
}