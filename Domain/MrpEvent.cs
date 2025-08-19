namespace Domain;

public class MrpEvent
{
    public string MrpId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public DateTime EventDate { get; set; }
    public string MrpEventTypeId { get; set; } = null!;
    public string? FacilityId { get; set; }
    public double? Quantity { get; set; }
    public string? EventName { get; set; }
    public string? IsLate { get; set; }
    public string? FacilityIdTo { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Facility? Facility { get; set; }
    public MrpEventType MrpEventType { get; set; } = null!;
    public Product Product { get; set; } = null!;
}