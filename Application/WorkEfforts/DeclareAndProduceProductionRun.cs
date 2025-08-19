using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Manufacturing;
using Application.WorkEfforts;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.WorkEfforts
{
    public class DeclareAndProduceProductionRun
    {
        public class Command : IRequest<Result<DeclareAndProduceProductionRunResult>>
        {
            public DeclareAndProduceProductionRunParams DeclareAndProduceProductionRunParams { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<DeclareAndProduceProductionRunResult>>
        {
            private readonly IProductionRunService _productionRunService;
            private readonly DataContext _context;

            public Handler(IProductionRunService productionRunService, DataContext context)
            {
                _productionRunService = productionRunService;
                _context = context;
            }

            public async Task<Result<DeclareAndProduceProductionRunResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Call the service method to declare and produce the production run
                    var result = await _productionRunService.ProductionRunDeclareAndProduce(
                        request.DeclareAndProduceProductionRunParams
                    );
                    
                    var affectedRecords = _context.ChangeTracker
                        .Entries()
                        .Where(e => e.State == EntityState.Added ||
                                    e.State == EntityState.Modified ||
                                    e.State == EntityState.Deleted)
                        .Select(e => new ChangeRecord
                        {
                            TableName = e.Entity.GetType().Name,
                            PKValues = string.Join(", ", e.Properties
                                .Where(p => p.Metadata.IsPrimaryKey())
                                .Select(p => $"{p.Metadata.Name}: {p.CurrentValue}")),
                            Operation = e.State.ToString()
                        })
                        .ToList();

                    foreach (var record in affectedRecords)
                    {
                        Console.WriteLine(record);
                    }


                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    // Return success response with the result
                    return Result<DeclareAndProduceProductionRunResult>.Success(result);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    // Handle exceptions and return failure response with an appropriate error message
                    var errorMessage = $"Error declaring and producing production run: {ex.Message}";
                    return Result<DeclareAndProduceProductionRunResult>.Failure(errorMessage);
                }
            }
        }
    }
}
