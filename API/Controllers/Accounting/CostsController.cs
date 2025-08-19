using Application.Accounting.Costs;
using Application.Shipments.Costs;
using Application.Catalog.Products;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class CostsController : BaseApiController
{
    [HttpGet("{productId}/getProductCostComponents")]
    public async Task<IActionResult> GetProductCostComponents(string productId)
    {
        return HandleResult(await Mediator.Send(new GetProductCostComponents.Query{ ProductId = productId }));
    }
    
    
    [HttpGet("{productionRunId}/getActualProductCostComponents")]
    public async Task<IActionResult> GetActualProductCostComponents(string productionRunId)
    {
        return HandleResult(await Mediator.Send(new GetActualProductCostComponents.Query{ ProductionRunId = productionRunId }));
    }
    
    [HttpGet("{productId}/getProductCostComponentCalcs")]
    public async Task<IActionResult> GetProductCostComponentCalcs(string productId)
    {
        return HandleResult(await Mediator.Send(new GetProductCostComponentCalcs.Query{ ProductId = productId }));
    }
    
    [HttpGet("{productId}/getRoutingTaskCostComponentCalcs")]
    public async Task<IActionResult> GetRoutingTaskCostComponentCalcs(string productId)
    {
        return HandleResult(await Mediator.Send(new GetRoutingTaskCostComponentCalcs.Query{ ProductId = productId }));
    }
    
    [HttpGet("{productId}/getRoutingTaskFixedAssetCosts")]
    public async Task<IActionResult> GetRoutingTaskFixedAssetCosts(string productId)
    {
        return HandleResult(await Mediator.Send(new GetRoutingTaskFixedAssetCosts.Query{ ProductId = productId }));
    }
    
    [HttpGet("{productId}/getMaterialCostConfig")]
    public async Task<IActionResult> GetMaterialCostConfig(string productId)
    {
        return HandleResult(await Mediator.Send(new GetMaterialCostConfig.Query{ ProductId = productId }));
    } 
    
    [HttpGet("{productId}/listTaskDirectLaborCalculations")]
    public async Task<IActionResult> ListTaskDirectLaborCalculations(string productId)
    {
        return HandleResult(await Mediator.Send(new ListTaskDirectLaborCalculations.Query{ ProductId = productId }));
    }
    
    [HttpGet("{productId}/listTaskFOHCalculations")]
    public async Task<IActionResult> ListTaskFOHCalculations(string productId)
    {
        return HandleResult(await Mediator.Send(new ListTaskFOHCalculations.Query{ ProductId = productId }));
    }
}