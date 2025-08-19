namespace Domain;

public class QuantityBreakType
{
    public QuantityBreakType()
    {
        QuantityBreaks = new HashSet<QuantityBreak>();
    }

    public string QuantityBreakTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<QuantityBreak> QuantityBreaks { get; set; }
}