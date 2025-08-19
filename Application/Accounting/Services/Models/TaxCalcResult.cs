namespace Application.Accounting.Services.Models;

public class TaxCalcResult
{
    public decimal TaxTotal { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal PriceWithTax { get; set; }
}