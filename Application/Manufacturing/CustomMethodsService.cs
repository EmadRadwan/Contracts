using Application.Catalog.Products.Services.Cost;

namespace Application.Manufacturing
{
    public class CustomMethodsService
    {
        private readonly ICostService _costService;

        // Inject ICostService through the constructor
        public CustomMethodsService(ICostService costService)
        {
            _costService = costService;
        }

        public async Task<decimal> ProductCostPercentageFormula(CustomMethodParameters parameters)
        {
            try
            {
                // Extract relevant fields from parameters
                var productCostComponentCalc = parameters.ProductCostComponentCalc;
                var costComponentCalc = parameters.CostComponentCalc;

                // Ensure required fields are not null
                if (productCostComponentCalc == null || costComponentCalc == null)
                {
                    throw new ArgumentNullException("ProductCostComponentCalc and CostComponentCalc cannot be null");
                }

                // Prepare input for the GetProductCost service
                string productId = productCostComponentCalc.ProductId;
                string currencyUomId = parameters.CurrencyUomId;
                string costComponentTypePrefix = parameters.CostComponentTypePrefix;

                // Call the GetProductCost service to dynamically retrieve the product cost
                decimal productCost = await _costService.GetProductCost(productId, currencyUomId, costComponentTypePrefix);

                // Perform the cost adjustment calculation
                decimal percentage = costComponentCalc.FixedCost ?? 0; // Default to 0 if FixedCost is null
                decimal productCostAdjustment = productCost * percentage;

                // Return the result
                return productCostAdjustment;
            }
            catch (Exception ex)
            {
                // Handle exceptions and log the error
                Console.WriteLine($"Error in ProductCostPercentageFormula: {ex.Message}");
                throw;
            }
        }
    }
}
