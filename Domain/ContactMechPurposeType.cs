using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class ContactMechPurposeType
{
    public ContactMechPurposeType()
    {
        ContactMechTypePurposes = new HashSet<ContactMechTypePurpose>();
        FacilityContactMechPurposes = new HashSet<FacilityContactMechPurpose>();
        InvoiceContactMeches = new HashSet<InvoiceContactMech>();
        OrderContactMeches = new HashSet<OrderContactMech>();
        OrderItemContactMeches = new HashSet<OrderItemContactMech>();
        PartyContactMechPurposes = new HashSet<PartyContactMechPurpose>();
        ReturnContactMeches = new HashSet<ReturnContactMech>();
    }

    public string ContactMechPurposeTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ContactMechTypePurpose> ContactMechTypePurposes { get; set; }
    public ICollection<FacilityContactMechPurpose> FacilityContactMechPurposes { get; set; }
    public ICollection<InvoiceContactMech> InvoiceContactMeches { get; set; }
    public ICollection<OrderContactMech> OrderContactMeches { get; set; }
    public ICollection<OrderItemContactMech> OrderItemContactMeches { get; set; }
    public ICollection<PartyContactMechPurpose> PartyContactMechPurposes { get; set; }
    public ICollection<ReturnContactMech> ReturnContactMeches { get; set; }
}