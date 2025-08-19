using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class RoleType
{
    public RoleType()
    {
        AcctgTrans = new HashSet<AcctgTran>();
        AcctgTransEntries = new HashSet<AcctgTransEntry>();
        CommunicationEventRoleTypeIdFromNavigations = new HashSet<CommunicationEvent>();
        CommunicationEventRoleTypeIdToNavigations = new HashSet<CommunicationEvent>();
        ContentApprovals = new HashSet<ContentApproval>();
        ContentPurposeOperations = new HashSet<ContentPurposeOperation>();
        FacilityParties = new HashSet<FacilityParty>();
        FixedAssets = new HashSet<FixedAsset>();
        InverseParentType = new HashSet<RoleType>();
        Invoices = new HashSet<Invoice>();
        PartyContactMeches = new HashSet<PartyContactMech>();
        PartyInvitationRoleAssocs = new HashSet<PartyInvitationRoleAssoc>();
        PartyNeeds = new HashSet<PartyNeed>();
        PartyRelationshipTypeRoleTypeIdValidFromNavigations = new HashSet<PartyRelationshipType>();
        PartyRelationshipTypeRoleTypeIdValidToNavigations = new HashSet<PartyRelationshipType>();
        PartyRoles = new HashSet<PartyRole>();
        Payments = new HashSet<Payment>();
        ProductContents = new HashSet<ProductContent>();
        ProductSubscriptionResources = new HashSet<ProductSubscriptionResource>();
        RoleTypeAttrs = new HashSet<RoleTypeAttr>();
        SalesOpportunityRoles = new HashSet<SalesOpportunityRole>();
        ShipmentCostEstimates = new HashSet<ShipmentCostEstimate>();
        SubscriptionOriginatedFromRoleTypes = new HashSet<Subscription>();
        SubscriptionRoleTypes = new HashSet<Subscription>();
        ValidContactMechRoles = new HashSet<ValidContactMechRole>();
    }

    public string RoleTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public bool IncludeInFilter { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public RoleType? ParentType { get; set; }
    public ICollection<AcctgTran> AcctgTrans { get; set; }
    public ICollection<AcctgTransEntry> AcctgTransEntries { get; set; }
    public ICollection<CommunicationEvent> CommunicationEventRoleTypeIdFromNavigations { get; set; }
    public ICollection<CommunicationEvent> CommunicationEventRoleTypeIdToNavigations { get; set; }
    public ICollection<ContentApproval> ContentApprovals { get; set; }
    public ICollection<ContentPurposeOperation> ContentPurposeOperations { get; set; }
    public ICollection<FacilityParty> FacilityParties { get; set; }
    public ICollection<FixedAsset> FixedAssets { get; set; }
    public ICollection<RoleType> InverseParentType { get; set; }
    public ICollection<Invoice> Invoices { get; set; }
    public ICollection<PartyContactMech> PartyContactMeches { get; set; }
    public ICollection<PartyInvitationRoleAssoc> PartyInvitationRoleAssocs { get; set; }
    public ICollection<PartyNeed> PartyNeeds { get; set; }
    public ICollection<PartyRelationshipType> PartyRelationshipTypeRoleTypeIdValidFromNavigations { get; set; }
    public ICollection<PartyRelationshipType> PartyRelationshipTypeRoleTypeIdValidToNavigations { get; set; }
    public ICollection<PartyRole> PartyRoles { get; set; }
    public ICollection<Payment> Payments { get; set; }
    public ICollection<ProductContent> ProductContents { get; set; }
    public ICollection<ProductSubscriptionResource> ProductSubscriptionResources { get; set; }
    public ICollection<RoleTypeAttr> RoleTypeAttrs { get; set; }
    public ICollection<SalesOpportunityRole> SalesOpportunityRoles { get; set; }
    public ICollection<ShipmentCostEstimate> ShipmentCostEstimates { get; set; }
    public ICollection<Subscription> SubscriptionOriginatedFromRoleTypes { get; set; }
    public ICollection<Subscription> SubscriptionRoleTypes { get; set; }
    public ICollection<ValidContactMechRole> ValidContactMechRoles { get; set; }
}