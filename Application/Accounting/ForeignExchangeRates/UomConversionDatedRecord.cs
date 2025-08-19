using System.ComponentModel.DataAnnotations;

namespace Application.Shipments.ForeignExchangeRates;

public class UomConversionDatedRecord
{
    [Key] public string UomId { get; set; }

    public string UomIdDescription { get; set; }

    [Key] public string UomIdTo { get; set; }

    public string UomIdToDescription { get; set; }

    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public double? ConversionFactor { get; set; }
    public string? CustomMethodId { get; set; }
    public int? DecimalScale { get; set; }
    public string? RoundingMode { get; set; }
    public string? PurposeEnumId { get; set; }
}