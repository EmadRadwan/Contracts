using Domain;

namespace Application.Accounting.Services.Models;

public class PriceCalcResult{
    public decimal BasePrice { get; set; }
    public decimal Price { get; set; }
    public decimal ListPrice { get; set; }
    public decimal DefaultPrice { get; set; }
    public decimal AverageCost { get; set; }
    public List<OrderItemPriceInfo> OrderItemPriceInfos { get; set; }
    public bool IsSale { get; set; }
    public bool ValidPriceFound { get; set; }
}
