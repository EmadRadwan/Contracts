using Domain;

namespace Application.Catalog.Products;

public class CustomPriceCalcInput
{
    public UserLogin UserLogin { get; set; }
    public Domain.Product Product { get; set; }
    public decimal? InitialPrice { get; set; }
    public string CurrencyUomId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Amount { get; set; }
    public string SurveyResponseId { get; set; }
}