namespace Domain;

public class FinAccountAuth
{
    public string FinAccountAuthId { get; set; } = null!;
    public string? FinAccountId { get; set; }
    public decimal? Amount { get; set; }
    public string? CurrencyUomId { get; set; }
    public DateTime? AuthorizationDate { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FinAccount? FinAccount { get; set; }
}