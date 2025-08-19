namespace Domain;

public class UomGroup
{
    public string UomGroupId { get; set; } = null!;
    public string UomId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom Uom { get; set; } = null!;
}