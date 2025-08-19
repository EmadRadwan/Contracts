using Domain;

namespace Application.Catalog.Products
{
    public class ProductPriceResult
    {
        public decimal BasePrice { get; set; }
        public decimal Price { get; set; }
        public decimal? DefaultPrice { get; set; }
        public decimal? ListPrice { get; set; }
        public decimal? CompetitivePrice { get; set; }
        public decimal? AverageCost { get; set; }
        public decimal? PromoPrice { get; set; }
        public decimal? SpecialPromoPrice { get; set; }
        public bool IsSale { get; set; }
        public bool ValidPriceFound { get; set; }
        public string CurrencyUsed { get; set; }
        public List<OrderItemPriceInfo> OrderItemPriceInfos { get; set; }
        public List<ProductPriceResult> AllQuantityPrices { get; set; }
        // For quantity break reference
        public ProductPriceRule QuantityProductPriceRule { get; set; }

        // Added property for error messages
        public string ErrorMessage { get; set; }
    }
}
