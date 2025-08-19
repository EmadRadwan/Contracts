using Application.Catalog.Products.Services.Cost;
using Domain;
using MediatR;
using Persistence;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Application.Manufacturing
{
    public class UpdateWorkEffortCostCalc
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
                _loggerForTransaction = Log.ForContext("Transaction", "update work effort cost calc");
            }

            public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    _loggerForTransaction.Information("UpdateWorkEffortCostCalc.cs starting");

                    // Find the existing WorkEffortCostCalc record
                    var workEffortCostCalc = await _context.WorkEffortCostCalcs
                        .FirstOrDefaultAsync(x =>
                            x.WorkEffortId == request.WorkEffortCostCalcDto.WorkEffortId &&
                            x.CostComponentCalcId == request.WorkEffortCostCalcDto.CostComponentCalcId &&
                            x.CostComponentTypeId == request.WorkEffortCostCalcDto.CostComponentTypeId,
                            cancellationToken);

                    if (workEffortCostCalc == null)
                    {
                        return Result<string>.Failure("WorkEffortCostCalc not found.");
                    }

                    // Update the record
                    workEffortCostCalc.CostComponentTypeId = request.WorkEffortCostCalcDto.CostComponentTypeId;
                    workEffortCostCalc.CostComponentCalcId = request.WorkEffortCostCalcDto.CostComponentCalcId;
                    workEffortCostCalc.FromDate = (DateTime)request.WorkEffortCostCalcDto.FromDate;
                    workEffortCostCalc.ThruDate = request.WorkEffortCostCalcDto.ThruDate;

                    var workEffortId = await _costService.UpdateWorkEffortCostCalc(workEffortCostCalc);

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _loggerForTransaction.Information("UpdateWorkEffortCostCalc.cs end - Success");
                    return Result<string>.Success(workEffortId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _loggerForTransaction.Information("UpdateWorkEffortCostCalc.cs error: {Error}", ex.Message);
                    return Result<string>.Failure($"Error updating work effort cost calc: {ex.Message}");
                }
            }
        }
    }
}