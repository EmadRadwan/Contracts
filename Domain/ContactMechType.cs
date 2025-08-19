using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class ContactMechType
{
    public ContactMechType()
    {
        CommunicationEventTypes = new HashSet<CommunicationEventType>();
        CommunicationEvents = new HashSet<CommunicationEvent>();
        ContactLists = new HashSet<ContactList>();
        ContactMechTypeAttrs = new HashSet<ContactMechTypeAttr>();
        ContactMechTypePurposes = new HashSet<ContactMechTypePurpose>();
        ContactMeches = new HashSet<ContactMech>();
        InverseParentType = new HashSet<ContactMechType>();
        ValidContactMechRoles = new HashSet<ValidContactMechRole>();
    }

    public string ContactMechTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMechType? ParentType { get; set; }
    public ICollection<CommunicationEventType> CommunicationEventTypes { get; set; }
    public ICollection<CommunicationEvent> CommunicationEvents { get; set; }
    public ICollection<ContactList> ContactLists { get; set; }
    public ICollection<ContactMechTypeAttr> ContactMechTypeAttrs { get; set; }
    public ICollection<ContactMechTypePurpose> ContactMechTypePurposes { get; set; }
    public ICollection<ContactMech> ContactMeches { get; set; }
    public ICollection<ContactMechType> InverseParentType { get; set; }
    public ICollection<ValidContactMechRole> ValidContactMechRoles { get; set; }
}