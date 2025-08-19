namespace Application.Uoms;

public class CurrencyDto
{
    public string CurrencyUomId { get; set; }
    public string UomTypeId { get; set; }
    public string Abbreviation { get; set; }
    public decimal? NumericCode { get; set; }
    public string Description { get; set; }
}