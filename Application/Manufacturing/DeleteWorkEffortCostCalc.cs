using Application.Catalog.Products.Services.Cost;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Manufacturing
{
    public class DeleteWorkEffortCostCalc
    {
        // DTO to match what frontend sends
        public class DeleteWorkEffortCostCalcDto
        {
            public string WorkEffortId { get; set; }
            public string CostComponentCalcId { get; set; }
            public string CostComponentTypeId { get; set; }
            public DateTime FromDate { get; set; } // parsed from frontend string
        }

        public class Command : IRequest<Result<string>>
        {
            public DeleteWorkEffortCostCalcDto Dto { get; set; }
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
                _loggerForTransaction = Log.ForContext("Transaction", "delete work effort cost calc");
            }

            public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    _loggerForTransaction.Information("DeleteWorkEffortCostCalc.cs starting");

                    // Find the record using DTO parameters
                    var workEffortCostCalc = await _context.WorkEffortCostCalcs
                        .FirstOrDefaultAsync(x =>
                            x.WorkEffortId == request.Dto.WorkEffortId &&
                            x.CostComponentCalcId == request.Dto.CostComponentCalcId &&
                            x.CostComponentTypeId == request.Dto.CostComponentTypeId &&
                            x.FromDate.Date == request.Dto.FromDate.Date,
                            cancellationToken);

                    if (workEffortCostCalc == null)
                        return Result<string>.Failure("WorkEffortCostCalc not found.");

                    // Perform delete
                    _context.WorkEffortCostCalcs.Remove(workEffortCostCalc);

                    // Optionally: call service if it contains extra logic
                    // await _costService.DeleteWorkEffortCostCalc(workEffortCostCalc);

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _loggerForTransaction.Information("DeleteWorkEffortCostCalc.cs end - Success");
                    return Result<string>.Success(request.Dto.WorkEffortId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _loggerForTransaction.Error(ex, "DeleteWorkEffortCostCalc.cs error");
                    return Result<string>.Failure($"Error deleting work effort cost calc: {ex.Message}");
                }
            }
        }
    }
}
