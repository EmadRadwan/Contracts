namespace Domain;

public class CountryCode
{
    public string CountryCode1 { get; set; } = null!;
    public string? CountryAbbr { get; set; }
    public string? CountryNumber { get; set; }
    public string? CountryName { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CountryCapital CountryCapital { get; set; } = null!;
    public CountryTeleCode CountryTeleCode { get; set; } = null!;
}