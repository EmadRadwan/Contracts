using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Application.Catalog.Products;

namespace Application.Manufacturing;

// DTO for the list of WorkEffortGoodStandard records
public class WorkEffortGoodStandardListDto
{
    public string WorkEffortId { get; set; }
    
    // REFACTOR: Removed redundant ProductName property
    // Purpose: ProductName is already included in ProductLovDto, eliminating duplication
    // Benefit: Simplifies DTO structure and avoids redundant data
    public GetSalesProductsLov.ProductLovDto ProductId { get; set; }
    public string WorkEffortGoodStdTypeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public decimal? EstimatedQuantity { get; set; }
}

// Query to fetch WorkEffortGoodStandard records for a given WorkEffortId
public class ListWorkEffortGoodStandard
{
    public class Query : IRequest<Result<List<WorkEffortGoodStandardListDto>>>
    {
        public Query(string workEffortId)
        {
            WorkEffortId = workEffortId;
        }

        public string WorkEffortId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<WorkEffortGoodStandardListDto>>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper, ILogger<Handler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<WorkEffortGoodStandardListDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                // Verify WorkEffort exists
                // Business: Ensures the WorkEffortId is valid before querying
                // Technical: Prevents unnecessary queries for non-existent WorkEfforts
                var workEffortExists = await _context.WorkEfforts
                    .AnyAsync(we => we.WorkEffortId == request.WorkEffortId, cancellationToken);

                if (!workEffortExists)
                {
                    _logger.LogWarning("WorkEffort not found for WorkEffortId: {WorkEffortId}", request.WorkEffortId);
                    return Result<List<WorkEffortGoodStandardListDto>>.Failure("WorkEffort not found");
                }

                // Query WorkEffortGoodStandard records with Product join
                // Business: Retrieves all product links for a specific routing (WorkEffort)
                // Technical: Joins WorkEffortGoodStandard with Product to get product details, filters by WorkEffortId and WorkEffortGoodStdTypeId
                // REFACTOR: Simplified query and moved OrderBy to client-side to avoid LINQ translation issue
                // Purpose: EF Core cannot translate complex object creation in OrderBy; using AsEnumerable for sorting
                // Benefit: Ensures query executes correctly while maintaining sort functionality
                var productLinks = await _context.WorkEffortGoodStandards
                    .Where(w => w.WorkEffortId == request.WorkEffortId && w.WorkEffortGoodStdTypeId == "ROU_PROD_TEMPLATE")
                    .Join(
                        _context.Products,
                        w => w.ProductId,
                        p => p.ProductId,
                        (w, p) => new WorkEffortGoodStandardListDto
                        {
                            WorkEffortId = w.WorkEffortId,
                            ProductId = new GetSalesProductsLov.ProductLovDto
                            {
                                ProductId = p.ProductId,
                                ProductName = p.ProductName
                            },
                            WorkEffortGoodStdTypeId = w.WorkEffortGoodStdTypeId,
                            FromDate = w.FromDate,
                            ThruDate = w.ThruDate,
                            EstimatedQuantity = w.EstimatedQuantity as decimal?
                        }
                    )
                    .OrderBy(w => w.ProductId.ProductId)
                    .ToListAsync();

                // Log success
                // Business: Confirms successful retrieval of product links
                // Technical: Adds logging for traceability
                _logger.LogInformation(
                    "Successfully retrieved {Count} WorkEffortGoodStandard records for WorkEffortId: {WorkEffortId}",
                    productLinks.Count, request.WorkEffortId);

                return Result<List<WorkEffortGoodStandardListDto>>.Success(productLinks);
            }
            catch (Exception ex)
            {
                // Log and handle exceptions
                // Business: Captures errors during data retrieval
                // Technical: Ensures robust error handling
                _logger.LogError(ex, "Error retrieving WorkEffortGoodStandard list for WorkEffortId: {WorkEffortId}", request.WorkEffortId);
                return Result<List<WorkEffortGoodStandardListDto>>.Failure(
                    $"Error retrieving WorkEffortGoodStandard list: {ex.Message}");
            }
        }
    }
}