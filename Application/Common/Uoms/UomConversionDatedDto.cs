namespace Application.Common.Uoms;

public class UomConversionDatedDto
{
    public string UomId { get; set; } = null!;
    public string UomIdTo { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public double? ConversionFactor { get; set; }
    public string? CustomMethodId { get; set; }
    public int? DecimalScale { get; set; }
    public string? RoundingMode { get; set; }
}