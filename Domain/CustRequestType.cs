using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class CustRequestType
{
    public CustRequestType()
    {
        CustRequestCategories = new HashSet<CustRequestCategory>();
        CustRequestResolutions = new HashSet<CustRequestResolution>();
        CustRequestTypeAttrs = new HashSet<CustRequestTypeAttr>();
        CustRequests = new HashSet<CustRequest>();
        InverseParentType = new HashSet<CustRequestType>();
    }

    public string CustRequestTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public string? PartyId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustRequestType? ParentType { get; set; }
    public Party? Party { get; set; }
    public ICollection<CustRequestCategory> CustRequestCategories { get; set; }
    public ICollection<CustRequestResolution> CustRequestResolutions { get; set; }
    public ICollection<CustRequestTypeAttr> CustRequestTypeAttrs { get; set; }
    public ICollection<CustRequest> CustRequests { get; set; }
    public ICollection<CustRequestType> InverseParentType { get; set; }
}