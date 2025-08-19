using System.ComponentModel.DataAnnotations;

namespace Application.Shipments.Taxes;

public class TaxAuthorityRecord
{
    [Key] public string TaxAuthGeoId { get; set; }

    public string TaxAuthGeoDescription { get; set; }
    public string TaxAuthPartyId { get; set; }
    public string TaxAuthPartyName { get; set; }
    public string? RequireTaxIdForExemption { get; set; }
    public string? TaxIdFormatPattern { get; set; }
    public string? IncludeTaxInPrice { get; set; }
}