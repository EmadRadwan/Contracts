namespace Domain;

public class PartyRateNew
{
    public string PartyId { get; set; } = null!;
    public string RateTypeId { get; set; } = null!;
    public string? DefaultRate { get; set; }
    public double? PercentageUsed { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
    public RateType RateType { get; set; } = null!;
}