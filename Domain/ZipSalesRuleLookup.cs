namespace Domain;

public class ZipSalesRuleLookup
{
    public string StateCode { get; set; } = null!;
    public string City { get; set; } = null!;
    public string County { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public string? IdCode { get; set; }
    public string? Taxable { get; set; }
    public string? ShipCond { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}