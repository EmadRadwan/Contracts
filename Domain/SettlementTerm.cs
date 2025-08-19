namespace Domain;

public class SettlementTerm
{
    public SettlementTerm()
    {
        AcctgTransEntries = new HashSet<AcctgTransEntry>();
    }

    public string SettlementTermId { get; set; } = null!;
    public string? TermName { get; set; }
    public int? TermValue { get; set; }
    public string? UomId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<AcctgTransEntry> AcctgTransEntries { get; set; }
}