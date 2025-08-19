using Application.Catalog.Products.Services.Cost;
using Domain;
using MediatR;
using Persistence;
using Serilog;

public class CreateWorkEffortCostCalc
{
    public class Command : IRequest<Result<string>>
    {
        public WorkEffortCostCalcDto WorkEffortCostCalcDto { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<string>>
    {
        private readonly DataContext _context;
        private readonly ICostService _costService;
        private readonly ILogger _loggerForTransaction;

        public Handler(ICostService costService, DataContext context)
        {
            _context = context;
            _costService = costService;
            _loggerForTransaction = Log.ForContext("Transaction", "create work effort cost calc");
        }

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _loggerForTransaction.Information("CreateWorkEffortCostCalc.cs starting");
                var workEffortCostCalc = new WorkEffortCostCalc
                {
                    WorkEffortId = request.WorkEffortCostCalcDto.WorkEffortId,
                    CostComponentCalcId = request.WorkEffortCostCalcDto.CostComponentCalcId,
                    CostComponentTypeId = request.WorkEffortCostCalcDto.CostComponentTypeId,
                    FromDate = (DateTime)request.WorkEffortCostCalcDto.FromDate,
                    ThruDate = request.WorkEffortCostCalcDto.ThruDate
                };
                var workEffortId = await _costService.CreateWorkEffortCostCalc(workEffortCostCalc);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                _loggerForTransaction.Information("CreateWorkEffortCostCalc.cs end - Success");
                return Result<string>.Success(workEffortId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _loggerForTransaction.Information("CreateWorkEffortCostCalc.cs error: {Error}", ex.Message);
                return Result<string>.Failure($"Error creating work effort cost calc: {ex.Message}");
            }
        }
    }
}

public class WorkEffortCostCalcDto
{
    public string WorkEffortId { get; set; }
    public string CostComponentCalcId { get; set; }
    public string CostComponentTypeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
}