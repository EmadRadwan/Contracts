using MediatR;
using Application.WorkEfforts;
using Persistence;
using Serilog;


namespace Application.Manufacturing
{
    public class CreateProductionRunsForProductBom
    {
        public class Command : IRequest<Result<CreateProductionRunsResult>>
        {
            public CreateProductionRunDto CreateProductionRunDto { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<CreateProductionRunsResult>>
        {
            private readonly IProductionRunHelperService _productionRunHelperService;
            private readonly DataContext _context;
            private readonly Serilog.ILogger loggerForTransaction;


            public Handler(IProductionRunHelperService productionRunHelperService, DataContext context)
            {
                _productionRunHelperService = productionRunHelperService;
                _context = context;
                
                loggerForTransaction = Log.ForContext("Transaction", "create production run for BOM");

            }

            public async Task<Result<CreateProductionRunsResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Call the service method to create production run
                    loggerForTransaction.Information("CreateProductionRunsForProductBom.cs starting");

                    var response = await _productionRunHelperService.CreateProductionRunsForProductBom(
                        request.CreateProductionRunDto.ProductId,
                        request.CreateProductionRunDto.EstimatedStartDate,
                        request.CreateProductionRunDto.QuantityToProduce,
                        request.CreateProductionRunDto.FacilityId,
                        request.CreateProductionRunDto.WorkEffortName,
                        request.CreateProductionRunDto.Description,
                        request.CreateProductionRunDto.RoutingId
                    ); 
                    
                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);
                    
                    loggerForTransaction.Information("CreateProductionRunsForProductBom.cs end - Success");


                    return Result<CreateProductionRunsResult>.Success(response);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    loggerForTransaction.Information("CreateProductionRunsForProductBom.cs error");

                    // Handle exceptions and return failure response
                    return Result<CreateProductionRunsResult>.Failure($"Error creating production run: {ex.Message}");
                }
            }
        }
    }
}
