using MediatR;
using Persistence;
using System.Threading;
using System.Threading.Tasks;
using Application.Catalog.Products.Services.Cost;
using Application.Catalog.ProductStores;
using Application.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace Application.Catalog.Products
{
    public class CalculateProductCosts
    {
        public class Query : IRequest<Result<List<CostComponentDto>>>
        {
            public string? ProductId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<CostComponentDto>>>
        {
            private readonly DataContext _context;
            private readonly ICostService _costService;
            private readonly IProductStoreService _productStoreService;

            public Handler(DataContext context, ICostService costService, IProductStoreService productStoreService)
            {
                _context = context;
                _costService = costService;
                _productStoreService = productStoreService;
            }

            public async Task<Result<List<CostComponentDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    var stamp = DateTime.Now;


                    // get base currency
                    var baseCurrencyId = await _productStoreService.GetAcctgBaseCurrencyId();


                    var productCosts =
                        await _costService.CalculateProductCosts(request.ProductId, baseCurrencyId.CurrencyUomId,
                            "EST_STD");

                    await _context.SaveChangesAsync(cancellationToken);


                    await transaction.CommitAsync(cancellationToken);

                    // select cost components for the product
                    // from CostComponents and project the result to CostComponentDto
                    // and join with CostComponentTypes to get the description of the cost component type
                    var productCostComponents = await _context.CostComponents
                        .Where(x => x.ProductId == request.ProductId)
                        .Select(x => new CostComponentDto
                        {
                            CostComponentId = x.CostComponentId,
                            CostComponentTypeId = x.CostComponentTypeId,
                            CostComponentTypeDescription = x.CostComponentType.Description,
                            ProductId = x.ProductId,
                            ProductFeatureId = x.ProductFeatureId,
                            PartyId = x.PartyId,
                            Cost = x.Cost,
                        }).ToListAsync(cancellationToken);

                    // Return the simulation result
                    return Result<List<CostComponentDto>>.Success(productCostComponents);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<List<CostComponentDto>>.Failure("Error Calculating Product Costs");
                }
            }
        }
    }
}

    // Calculate Product Costs (CalculateProductCosts)
    //1 - Calls CancelCostComponents twice
    // with costComponentTypeId = 'EST_STD_ROUTE_COST'
    // and costComponentTypeId = 'EST_STD_MAT_COST'
    // This function expires all cost components with the given type
    // for a product
    
    //2 - Calls GetManufacturingComponents with productId
    // 2.1 The function starts by fetching BOM components for the product
    // ProductAssocs with ProductAssocTypeId = 'MANUF_COMPONENT'
    // and FromDate <= fromDate and (ThruDate == null || ThruDate >= fromDate)
    // to select the last valid set of records
    // it returns the listof components and the routing information
    
    //2.2 Calls GetProductRouting with productId
    // this function fetches the routing information for the product
    // from table WorkEffortGoodStandards.
    // The function also fetches the tasks associated with the routing
    
    //3 - Calls GetProductCost with productId of each BOM component
    //, baseCurrencyId.CurrencyUomId, 'EST_STD'
    // This function calculates the cost of a product
    // it fetches the cost of the product from the CostComponent table
    // and sums the cost of the product's components
    // Currently only 'EST_STD_MAT_COST' is defined for each row material
    
    // CalculateProductCosts proceeds to calculate
    // totalProductsCost += component.Quantity * productCost

    //4 - Calls GetProductRouting with productId of the 
    // main product. This call is redundant as the routing
    // information was already fetched in step 2.2

    // CalculateProductCosts proceeds to calculate
    // task costs by calling GetTaskCost with the task

    //5 - Calls GetTaskCost with the task
    // 5.1 starts by calling GetEstimatedTaskTime with the task
    // GetEstimatedTaskTime fetches the task from WorkEfforts
    // and returns both EstimatedMilliSeconds and EstimatedSetupMillis

    // GetTaskCost proceeds to fetch the FixedAsset of the task
    // and calculated both the setupCost and the usageCost
    // by first fetching related records from FixedAssetStdCosts
    // and then calculating the cost based on
    // taskCost = (usageCost?.Amount ?? 0) * estimatedTaskTime + (setupCost?.Amount ?? 0) * setupTime;

    // GetTaskCost proceeds to calculate the cost of the task
    // by fetching related records from WorkEffortCostCalcs
    // for the task where this can return multiple records

    // for each WorkEffortCostCalc record GetTaskCost proceeds
    // by fetching the related CostComponentCalc record
    // and calculating the cost of the task using 
    // var totalCostComponentTime = totalEstimatedTaskTime / costComponentCalc.PerMilliSecond;
    // totalCostComponentCost = totalCostComponentTime * costComponentCalc.VariableCost +
    // costComponentCalc.FixedCost;

    // GetTaskCost proceeds to calculate the total cost of the task
    // it creates a CostByType list as part of the result
    // and as it iterates over the WorkEffortCostCalc records
    // it adds or updates the cost of the task to the list

    // CalculateProductCosts proceeds to calculate the total cost of the product
    // by summing the cost of the product's components and the cost of the tasks
    

    //6- CalculateProductCosts proceeds to loop
    // over taskCostResult.CostsByTyp from GetTaskCost
    // and calls RecreateCostComponent for each CostByType


    //7 -  CalculateProductCosts proceeds to RecreateCostComponent twice
    // with costComponentTypeId = 'EST_STD_ROUTE_COST'  and costComponentTypeId = 'EST_STD_MAT_COST'
    
    // CalculateProductCosts proceeds by fetching ProductCostComponentCalcs records
    // for the main product and calculating the cost of the product
    // based on the related CustomMethodId if one defined



    
    



    
    
    