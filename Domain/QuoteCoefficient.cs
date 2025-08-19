namespace Domain;

public class QuoteCoefficient
{
    public string QuoteId { get; set; } = null!;
    public string CoeffName { get; set; } = null!;
    public decimal? CoeffValue { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Quote Quote { get; set; } = null!;
}