namespace Domain;

public class AddressMatchMap
{
    public string MapKey { get; set; } = null!;
    public string MapValue { get; set; } = null!;
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}