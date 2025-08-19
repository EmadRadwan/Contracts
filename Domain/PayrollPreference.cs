namespace Domain;

public class PayrollPreference
{
    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public string PayrollPreferenceSeqId { get; set; } = null!;
    public string? DeductionTypeId { get; set; }
    public string? PaymentMethodTypeId { get; set; }
    public string? PeriodTypeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public double? Percentage { get; set; }
    public decimal? FlatAmount { get; set; }
    public string? RoutingNumber { get; set; }
    public string? AccountNumber { get; set; }
    public string? BankName { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
    public PartyRole PartyRole { get; set; } = null!;
}