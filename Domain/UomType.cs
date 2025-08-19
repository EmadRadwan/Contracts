using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class UomType
{
    public UomType()
    {
        InverseParentType = new HashSet<UomType>();
        Products = new HashSet<Product>();
        Uoms = new HashSet<Uom>();
    }

    public string UomTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UomType? ParentType { get; set; }
    public ICollection<UomType> InverseParentType { get; set; }
    public ICollection<Product> Products { get; set; }
    public ICollection<Uom> Uoms { get; set; }
}