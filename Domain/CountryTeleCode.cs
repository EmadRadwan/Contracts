namespace Domain;

public class CountryTeleCode
{
    public string CountryCode { get; set; } = null!;
    public string? TeleCode { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CountryCode CountryCodeNavigation { get; set; } = null!;
}