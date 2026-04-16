namespace Application.Common.Uoms;

public class UomConversionDatedDto
{
    public string UomId { get; set; } = null!;
    public string UomIdTo { get; set; } = null!;
    public DateOnly? FromDate { get; set; }
    public DateOnly? ThruDate { get; set; }
    public double? ConversionFactor { get; set; }
    public string? CustomMethodId { get; set; }
    public int? DecimalScale { get; set; }
    public string? RoundingMode { get; set; }
}