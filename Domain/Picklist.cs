namespace Domain;

public class Picklist
{
    public Picklist()
    {
        PicklistBins = new HashSet<PicklistBin>();
        PicklistRoles = new HashSet<PicklistRole>();
        PicklistStatusHistories = new HashSet<PicklistStatusHistory>();
        PicklistStatuses = new HashSet<PicklistStatus>();
    }

    public string PicklistId { get; set; } = null!;
    public string? Description { get; set; }
    public string? FacilityId { get; set; }
    public string? ShipmentMethodTypeId { get; set; }
    public string? StatusId { get; set; }
    public DateTime? PicklistDate { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Facility? Facility { get; set; }
    public ShipmentMethodType? ShipmentMethodType { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<PicklistBin> PicklistBins { get; set; }
    public ICollection<PicklistRole> PicklistRoles { get; set; }
    public ICollection<PicklistStatusHistory> PicklistStatusHistories { get; set; }
    public ICollection<PicklistStatus> PicklistStatuses { get; set; }
}