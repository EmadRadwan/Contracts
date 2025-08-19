using Application.Catalog.Products;
using Application.Manufacturing;
using Application.WorkEfforts;
using Microsoft.AspNetCore.Mvc;
using CostComponentCalcDto = Application.Manufacturing.CostComponentCalcDto;

namespace API.Controllers.WorkEffort;

public class WorkEffortController : BaseApiController
{
    [HttpGet("{productId}/getProductRoutings")]
    public async Task<IActionResult> GetProductRoutings(string productId)
    {
        return HandleResult(await Mediator.Send(new GetProductRoutings.Query { ProductId = productId }));
    }

    [HttpGet("{workEffortId}/getProductionRunMaterials")]
    public async Task<IActionResult> GetProductionRunMaterials(string workEffortId)
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new GetProductionRunMaterials.Query
            { WorkEffortId = workEffortId, Language = language }));
    }

    [HttpGet("{workEffortId}/listIssueProductionRunDeclComponents")]
    public async Task<IActionResult> ListIssueProductionRunDeclComponents(string workEffortId)
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListIssueProductionRunDeclComponents.Query
            { WorkEffortId = workEffortId, Language = language }));
    }

    [HttpGet("{workEffortId}/listProducedProductionRunInventory")]
    public async Task<IActionResult> ListProducedProductionRunInventory(string workEffortId)
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListProducedProductionRunInventory.Query
            { WorkEffortId = workEffortId, Language = language }));
    }

    [HttpGet("{workEffortId}/getProductionRunComponentsForReturn")]
    public async Task<IActionResult> GetProductionRunComponentsForReturn(string workEffortId)
    {
        return HandleResult(await Mediator.Send(new GetProductionRunComponentsForReturn.Query
            { WorkEffortId = workEffortId }));
    }

    [HttpGet("{workEffortId}/getProductionRunParties")]
    public async Task<IActionResult> GetProductionRunParties(string workEffortId)
    {
        return HandleResult(await Mediator.Send(new GetProductionRunParties.Query { WorkEffortId = workEffortId }));
    }


    [HttpGet("{productionRunId}/getRoutingTasksForProductionRun")]
    public async Task<IActionResult> getRoutingTasksForProductionRun(string productionRunId)
    {
        return HandleResult(await Mediator.Send(new GetRoutingTasksForProductionRun.Query
            { ProductionRunId = productionRunId }));
    }

    [HttpGet("{productionRunId}/getRoutingTasksForProductionRunSimple")]
    public async Task<IActionResult> getRoutingTasksForProductionRunSimple(string productionRunId)
    {
        return HandleResult(await Mediator.Send(new GetRoutingTasksForProductionRunSimple.Query
            { ProductionRunId = productionRunId }));
    }

    [HttpPost("createProductionRun", Name = "CreateProductionRun")]
    public async Task<IActionResult> CreateProductionRun(CreateProductionRunDto createProductionRunDto)
    {
        return HandleResult(await Mediator.Send(new CreateProductionRun.Command
            { CreateProductionRunDto = createProductionRunDto }));
    }

    [HttpPost("createProductionRunsForProductBom", Name = "CreateProductionRunsForProductBom")]
    public async Task<IActionResult> CreateProductionRunsForProductBom(CreateProductionRunDto createProductionRunDto)
    {
        return HandleResult(await Mediator.Send(new CreateProductionRunsForProductBom.Command
            { CreateProductionRunDto = createProductionRunDto }));
    }

    [HttpPut("updateProductionRun", Name = "UpdateProductionRun")]
    public async Task<IActionResult> UpdateProductionRun(UpdateProductionRunDto updateProductionRunDto)
    {
        return HandleResult(await Mediator.Send(new UpdateProductionRun.Command
            { UpdateProductionRunDto = updateProductionRunDto }));
    }

    [HttpPut("changeProductionRunStatus", Name = "ChangeProductionRunStatus")]
    public async Task<IActionResult> ChangeProductionRunStatus(
        ChangeProductionRunStatusDto changeProductionRunStatusDto)
    {
        return HandleResult(await Mediator.Send(new ChangeProductionRunStatus.Command
            { ChangeProductionRunStatusDto = changeProductionRunStatusDto }));
    }

    [HttpPut("quickChangeProductionRunStatus", Name = "QuickChangeProductionRunStatus")]
    public async Task<IActionResult> QuickChangeProductionRunStatus(
        [FromBody] QuickChangeProductionRunStatusDto quickStatus)
    {
        return HandleResult(await Mediator.Send(new QuickChangeProductionRunStatus.Command
        {
            ProductionRunId = quickStatus.ProductionRunId,
            StatusId = quickStatus.StatusId,
            StartAllTasks = quickStatus.StartAllTasks
        }));
    }


    [HttpPut("changeProductionRunTaskStatus", Name = "ChangeProductionRunTaskStatus")]
    public async Task<IActionResult> ChangeProductionRunTaskStatus(
        ChangeProductionRunTaskStatusDto changeProductionRunTaskStatusDto)
    {
        return HandleResult(await Mediator.Send(new ChangeProductionRunTaskStatus.Command
            { ChangeProductionRunTaskStatusDto = changeProductionRunTaskStatusDto }));
    }

    [HttpPut("issueProductionRunTask", Name = "IssueProductionRunTask")]
    public async Task<IActionResult> IssueProductionRunTask(IssueProductionRunTaskParams issueProductionRunTaskParams)
    {
        return HandleResults(await Mediator.Send(new IssueProductionRunTask.Command
            { IssueProductionRunTaskParams = issueProductionRunTaskParams }));
    }

    [HttpPut("reserveProductionRunTask", Name = "ReserveProductionRunTask")]
    public async Task<IActionResult> ReserveProductionRunTask(ReserveProductionRunTaskParams param)
    {
        return HandleResult(await Mediator.Send(new ReserveProductionRunTask.Command
        {
            ReserveProductionRunTaskParams = param
        }));
    }


    [HttpPut("declareAndProduceProductionRun", Name = "DeclareAndProduceProductionRun")]
    public async Task<IActionResult> DeclareAndProduceProductionRun(
        DeclareAndProduceProductionRunParams declareAndProduceProductionRunParams)
    {
        return HandleResult(await Mediator.Send(new DeclareAndProduceProductionRun.Command
            { DeclareAndProduceProductionRunParams = declareAndProduceProductionRunParams }));
    }

    [HttpPut("updateProductionRunTask", Name = "UpdateProductionRunTask")]
    public async Task<IActionResult> UpdateProductionRunTask(
        UpdateProductionRunTaskContext UpdateProductionRunTaskContext)
    {
        return HandleResult(await Mediator.Send(new UpdateProductionRunTask.Command
            { UpdateProductionRunTaskContext = UpdateProductionRunTaskContext }));
    }

    [HttpPost("returnMaterials")]
    public async Task<IActionResult> ReturnMaterials([FromBody] ReturnMaterialsCommand command)
    {
        if (command == null || string.IsNullOrWhiteSpace(command.ProductionRunId) || command.Items == null ||
            !command.Items.Any())
        {
            return BadRequest("Invalid request: ProductionRunId and at least one item are required.");
        }

        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    [HttpGet("{mainProductionRunId}/wip-status")]
    public async Task<IActionResult> GetWipStatus(string mainProductionRunId, [FromQuery] string finishedProductId)
    {
        var query = new GetProductionRunWipStatus.Query
        {
            MainProductionRunId = mainProductionRunId,
            FinishedProductId = finishedProductId
        };

        var result = await Mediator.Send(query);

        return Ok(result.Value);
    }

    [HttpPost("createRouting")]
    public async Task<IActionResult> CreateRouting([FromBody] CreateRoutingDto dto)
    {
        var command = new CreateRouting.Command { CreateRoutingDto = dto };

        // REFACTOR: Dispatch command to MediatR handler and handle Result<string> response
        var result = await Mediator.Send(command);

        // REFACTOR: Return appropriate HTTP response based on result, mirroring standard API patterns
        return result.IsSuccess
            ? Ok(new { workEffortId = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("updateRouting")]
    public async Task<IActionResult> UpdateRouting([FromBody] UpdateRoutingDto dto)
    {
        var command = new UpdateRouting.Command { UpdateRoutingDto = dto };
        var result = await Mediator.Send(command);
        return result.IsSuccess
            ? Ok(new { workEffortId = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("createRoutingTask")]
    public async Task<IActionResult> CreateRoutingTask([FromBody] CreateRoutingTaskDto dto)
    {
        // REFACTOR: Dispatch command to MediatR handler and handle Result<string> response
        var command = new CreateRoutingTask.Command { CreateRoutingTaskDto = dto };
        var result = await Mediator.Send(command);

        // REFACTOR: Return appropriate HTTP response based on result
        return result.IsSuccess
            ? Ok(new { workEffortId = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("updateRoutingTask")]
    public async Task<IActionResult> UpdateRoutingTask([FromBody] UpdateRoutingTaskDto dto)
    {
        // REFACTOR: Dispatch command to MediatR handler and handle Result<string> response
        var command = new UpdateRoutingTask.Command { UpdateRoutingTaskDto = dto };
        var result = await Mediator.Send(command);

        // REFACTOR: Return appropriate HTTP response based on result
        return result.IsSuccess
            ? Ok(new { workEffortId = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("createWorkEffortAssoc", Name = "CreateWorkEffortAssoc")]
    public async Task<IActionResult> CreateWorkEffortAssoc(CreateWorkEffortAssoc.WorkEffortAssocDto workEffortAssoc)
    {
        // Validate the input DTO
        // Business: Ensures the request contains valid data for creating a WorkEffortAssoc
        // Technical: Checks for null DTO to prevent invalid requests
        if (workEffortAssoc == null)
        {
            // Business: Prevents processing of invalid or missing input
            // Technical: Returns BadRequest for null input, consistent with REST API practices
            return BadRequest("WorkEffortAssoc data is required.");
        }

        // Create the command from the DTO
        // Business: Maps API input to the MediatR command for processing
        // Technical: Translates DTO properties to command constructor parameters
        // REFACTOR: Replaced OFBiz's service input map with a DTO and command for better type safety
        var command = new CreateWorkEffortAssoc.Command(
            workEffortIdFrom: workEffortAssoc.WorkEffortIdFrom,
            workEffortIdTo: workEffortAssoc.WorkEffortIdTo,
            workEffortAssocTypeId: workEffortAssoc.WorkEffortAssocTypeId,
            fromDate: workEffortAssoc.FromDate,
            sequenceNum: workEffortAssoc.SequenceNum
        );

        // Send the command to the MediatR pipeline
        // Business: Delegates the creation logic to the handler for processing
        // Technical: Uses MediatR to execute the command asynchronously
        var result = await Mediator.Send(command);

        // Return the result using HandleResult
        // Business: Communicates success or failure of the WorkEffortAssoc creation to the client
        // Technical: Translates Result<Unit> to an IActionResult, following the CreateProduct syntax guide
        // REFACTOR: Leverages HandleResult for consistent result handling, aligning with ASP.NET Core conventions
        return HandleResult(result);
    }

    [HttpGet("getRoutingTaskAssocs/{workEffortId}", Name = "GetRoutingTaskAssocs")]
    public async Task<IActionResult> GetRoutingTaskAssocs(string workEffortId)
    {
        if (string.IsNullOrEmpty(workEffortId))
        {
            return BadRequest("WorkEffortId is required.");
        }

        var result = await Mediator.Send(new GetRoutingTaskAssocs.Query { WorkEffortId = workEffortId });

        return HandleResult(result);
    }

    [HttpGet("getRoutingTasksLov", Name = "GetRoutingTasksLov")]
    public async Task<IActionResult> GetRoutingTasksLov([FromQuery] GetRoutingTasksLov.RoutingTaskLovParams param)
    {
        var result = await Mediator.Send(new GetRoutingTasksLov.Query { Params = param });
        return HandleResult(result);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteWorkEffortAssoc([FromBody] DeleteWorkEffortAssoc.Command command)
    {
        // REFACTOR: Used FromBody to accept JSON payload, aligning with RESTful API standards
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    [HttpPut("updateWorkEffortAssoc", Name = "UpdateWorkEffortAssoc")]
    public async Task<IActionResult> UpdateWorkEffortAssoc(
        [FromBody] UpdateWorkEffortAssoc.WorkEffortAssocDto workEffortAssoc)
    {
        // Validate the input DTO
        // Business: Ensures the request contains valid data for updating a WorkEffortAssoc
        // Technical: Checks for null DTO to prevent invalid requests
        if (workEffortAssoc == null)
        {
            return BadRequest("WorkEffortAssoc data is required.");
        }

        // Create the command from the DTO
        // Business: Maps API input to the MediatR command for processing
        // Technical: Translates DTO properties to command constructor parameters
        var command = new UpdateWorkEffortAssoc.Command(
            workEffortIdFrom: workEffortAssoc.WorkEffortIdFrom,
            workEffortIdTo: workEffortAssoc.WorkEffortIdTo,
            workEffortAssocTypeId: workEffortAssoc.WorkEffortAssocTypeId,
            fromDate: workEffortAssoc.FromDate,
            sequenceNum: workEffortAssoc.SequenceNum,
            thruDate: workEffortAssoc.ThruDate
        );

        // Send the command to the MediatR pipeline
        // Business: Delegates the update logic to the handler for processing
        // Technical: Uses MediatR to execute the command asynchronously
        var result = await Mediator.Send(command);

        // Return the result using HandleResult
        // Business: Communicates success or failure of the WorkEffortAssoc update to the client
        // Technical: Translates Result<Unit> to an IActionResult
        return HandleResult(result);
    }

    [HttpPost("createWorkEffortGoodStandard")]
    public async Task<IActionResult> CreateWorkEffortGoodStandard(
        [FromBody] WorkEffortGoodStandardDto workEffortGoodStandard)
    {
        // Validate the input DTO
        // Business: Ensures valid data for creating a WorkEffortGoodStandard
        // Technical: Checks for null DTO
        // REFACTOR: Added null check for DTO to align with REST API standards
        if (workEffortGoodStandard == null)
        {
            return BadRequest("WorkEffortGoodStandard data is required.");
        }

        // Create the command from the DTO
        // Business: Maps API input to MediatR command
        // Technical: Translates DTO to command parameters
        // REFACTOR: Structured command creation to match OFBiz service inputs
        var command = new CreateWorkEffortGoodStandard.Command(
            workEffortId: workEffortGoodStandard.WorkEffortId,
            productId: workEffortGoodStandard.ProductId,
            workEffortGoodStdTypeId: workEffortGoodStandard.WorkEffortGoodStdTypeId,
            fromDate: workEffortGoodStandard.FromDate,
            estimatedQuantity: workEffortGoodStandard.EstimatedQuantity as decimal?
        );

        // Send the command to the MediatR pipeline
        // Business: Delegates creation logic to handler
        // Technical: Executes command asynchronously
        var result = await Mediator.Send(command);

        // Return the result
        // Business: Communicates success or failure to the client
        // Technical: Uses HandleResult for consistent response handling
        // REFACTOR: Leverages HandleResult for standardized API responses
        return HandleResult(result);
    }

    [HttpPut("updateWorkEffortGoodStandard")]
    public async Task<IActionResult> UpdateWorkEffortGoodStandard(
        [FromBody] WorkEffortGoodStandardDto workEffortGoodStandard)
    {
        // Validate the input DTO
        // Business: Ensures valid data for updating a WorkEffortGoodStandard
        // Technical: Checks for null DTO
        // REFACTOR: Added null check for DTO
        if (workEffortGoodStandard == null)
        {
            return BadRequest("WorkEffortGoodStandard data is required.");
        }

        // Create the command from the DTO
        // Business: Maps API input to MediatR command
        // Technical: Translates DTO to command parameters
        // REFACTOR: Structured command creation to match OFBiz service inputs
        var command = new UpdateWorkEffortGoodStandard.Command(
            workEffortId: workEffortGoodStandard.WorkEffortId,
            productId: workEffortGoodStandard.ProductId,
            workEffortGoodStdTypeId: workEffortGoodStandard.WorkEffortGoodStdTypeId,
            fromDate: workEffortGoodStandard.FromDate,
            thruDate: workEffortGoodStandard.ThruDate,
            estimatedQuantity: workEffortGoodStandard.EstimatedQuantity as decimal?
        );

        // Send the command to the MediatR pipeline
        // Business: Delegates update logic to handler
        // Technical: Executes command asynchronously
        var result = await Mediator.Send(command);

        // Return the result
        // Business: Communicates success or failure to the client
        // Technical: Uses HandleResult for consistent response handling
        // REFACTOR: Leverages HandleResult for standardized API responses
        return HandleResult(result);
    }

    [HttpPost("deleteWorkEffortGoodStandard")]
    public async Task<IActionResult> DeleteWorkEffortGoodStandard(
        [FromBody] DeleteWorkEffortGoodStandard.Command command)
    {
        // Send the command to the MediatR pipeline
        // Business: Delegates deletion logic to handler
        // Technical: Executes command asynchronously
        // REFACTOR: Used FromBody to accept JSON payload, aligning with RESTful API standards
        var result = await Mediator.Send(command);

        // Return the result
        // Business: Communicates success or failure to the client
        // Technical: Uses HandleResult for consistent response handling
        return HandleResult(result);
    }

    [HttpGet("getRoutingProductLinks/{workEffortId}")]
    public async Task<IActionResult> GetRoutingProductLinks(string workEffortId)
    {
        // Create the query
        // Business: Fetches the list of product links for a given routing
        // Technical: Maps the API request to the MediatR query
        // REFACTOR: Added explicit endpoint for fetching WorkEffortGoodStandard records
        var query = new ListWorkEffortGoodStandard.Query(workEffortId);

        // Send the query to the MediatR pipeline
        // Business: Delegates retrieval logic to handler
        // Technical: Executes query asynchronously
        var result = await Mediator.Send(query);

        // Return the result
        // Business: Communicates the list of product links to the client
        // Technical: Uses HandleResult for consistent response handling
        // REFACTOR: Leverages HandleResult for standardized API responses
        return HandleResult(result);
    }

    [HttpPost("createCostComponentCalc")]
    public async Task<IActionResult> CreateCostComponentCalc([FromBody] CostComponentCalcDto dto)
    {
        var command = new CreateCostComponentCalc.Command { CostComponentCalcDto = dto };
        var result = await Mediator.Send(command);
        return result.IsSuccess
            ? Ok(new { costComponentCalcId = result.Value })
            : BadRequest(new { error = result.Error });
    }
    
    [HttpPost("createCostComponent")]
    public async Task<IActionResult> CreateCostComponent([FromBody] CostComponentDto dto)
    {
        // REFACTOR: Create command and send to handler, mirroring CreateCostComponentCalc
        var command = new CreateCostComponent.Command { CostComponentDto = dto };
        var result = await Mediator.Send(command);

        return result.IsSuccess
            ? Ok(new { costComponentId = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("updateCostComponentCalc")]
    public async Task<IActionResult> UpdateCostComponentCalc([FromBody] CostComponentCalcDto dto)
    {
        var command = new UpdateCostComponentCalc.Command { CostComponentCalcDto = dto };
        var result = await Mediator.Send(command);
        return result.IsSuccess
            ? Ok(new { costComponentCalcId = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("createWorkEffortCostCalc")]
    public async Task<IActionResult> CreateWorkEffortCostCalc([FromBody] WorkEffortCostCalcDto dto)
    {
        var command = new CreateWorkEffortCostCalc.Command { WorkEffortCostCalcDto = dto };
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(new { workEffortId = result.Value }) : BadRequest(new { error = result.Error });
    }
    
    [HttpPut("updateWorkEffortCostCalc")]
    public async Task<IActionResult> UpdateWorkEffortCostCalc([FromBody] WorkEffortCostCalcDto dto)
    {
        var command = new UpdateWorkEffortCostCalc.Command { WorkEffortCostCalcDto = dto };
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(new { workEffortId = result.Value }) : BadRequest(new { error = result.Error });
    }
    
    [HttpDelete("deleteWorkEffortCostCalc")]
    public async Task<IActionResult> DeleteWorkEffortCostCalc(
        [FromBody] DeleteWorkEffortCostCalc.DeleteWorkEffortCostCalcDto dto)
    {
        var command = new DeleteWorkEffortCostCalc.Command
        {
            Dto = dto
        };

        var result = await Mediator.Send(command);

        return result.IsSuccess
            ? Ok(new { workEffortId = result.Value })
            : BadRequest(new { error = result.Error });
    }


    [HttpGet("getWorkEffortCostCalcs/{workEffortId}", Name = "GetWorkEffortCostCalcs")]
    public async Task<IActionResult> GetWorkEffortCostCalcs(string workEffortId)
    {
        if (string.IsNullOrEmpty(workEffortId))
        {
            return BadRequest("workEffortId is required");
        }

        var result = await Mediator.Send(new GetWorkEffortCostCalcs.Query { WorkEffortId = workEffortId });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
    
    [HttpGet("{workEffortId}")]
    public async Task<IActionResult> GetWorkEffort(string workEffortId)
    {
        var query = new GetWorkEffort.Query { WorkEffortId = workEffortId };
        var result = await Mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }
    
    [HttpGet("{workEffortId}/inventoryItems")]
    public async Task<IActionResult> GetBomInventoryItems(string workEffortId)
    {
        var language = GetLanguage();
        var result = await Mediator.Send(new GetBomInventoryItems.Query
            { WorkEffortId = workEffortId, Language = language });
        return HandleResult(result);
    }
}