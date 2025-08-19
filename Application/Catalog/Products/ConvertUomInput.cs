namespace Application.Catalog.Products;

public class ConvertUomInput
{
    public string UomId { get; set; }
    public string UomIdTo { get; set; }
    public decimal OriginalValue { get; set; }
    public long DefaultDecimalScale { get; set; }
    public string DefaultRoundingMode { get; set; }
}