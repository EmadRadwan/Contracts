using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class Agreement
{
    public Agreement()
    {
        Addenda = new HashSet<Addendum>();
        AgreementAttributes = new HashSet<AgreementAttribute>();
        AgreementContents = new HashSet<AgreementContent>();
        AgreementGeographicalApplics = new HashSet<AgreementGeographicalApplic>();
        AgreementItems = new HashSet<AgreementItem>();
        AgreementPartyApplics = new HashSet<AgreementPartyApplic>();
        AgreementRoles = new HashSet<AgreementRole>();
        AgreementStatuses = new HashSet<AgreementStatus>();
        AgreementTerms = new HashSet<AgreementTerm>();
        AgreementWorkEffortApplics = new HashSet<AgreementWorkEffortApplic>();
        OrderItemShipGroups = new HashSet<OrderItemShipGroup>();
    }

    public string AgreementId { get; set; } = null!;
    public string? ProductId { get; set; }
    public string? PartyIdFrom { get; set; }
    public string? PartyIdTo { get; set; }
    public string? RoleTypeIdFrom { get; set; }
    public string? RoleTypeIdTo { get; set; }
    public string? AgreementTypeId { get; set; }
    public DateTime? AgreementDate { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? Description { get; set; }
    public string? TextData { get; set; }
    public string? StatusId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public AgreementType? AgreementType { get; set; }
    public PartyRole? PartyRole { get; set; }
    public PartyRole? PartyRoleNavigation { get; set; }
    public Product? Product { get; set; }
    public ICollection<Addendum> Addenda { get; set; }
    public ICollection<AgreementAttribute> AgreementAttributes { get; set; }
    public ICollection<AgreementContent> AgreementContents { get; set; }
    public ICollection<AgreementGeographicalApplic> AgreementGeographicalApplics { get; set; }
    public ICollection<AgreementItem> AgreementItems { get; set; }
    public ICollection<AgreementPartyApplic> AgreementPartyApplics { get; set; }
    public ICollection<AgreementRole> AgreementRoles { get; set; }
    public ICollection<AgreementStatus> AgreementStatuses { get; set; }
    public ICollection<AgreementTerm> AgreementTerms { get; set; }
    public ICollection<AgreementWorkEffortApplic> AgreementWorkEffortApplics { get; set; }
    public ICollection<OrderItemShipGroup> OrderItemShipGroups { get; set; }
}