namespace Domain;

public class VehicleContent
{
    public string ContentId { get; set; }

    // Foreign key
    public string VehicleId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }

    // Navigation properties
    public virtual Vehicle Vehicle { get; set; }
    public virtual Content Content { get; set; }
}