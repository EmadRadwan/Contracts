namespace Domain;

public class CountryCapital
{
    public string CountryCode { get; set; } = null!;
    public string? CountryCapital1 { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CountryCode CountryCodeNavigation { get; set; } = null!;
}