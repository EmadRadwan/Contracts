using MediatR;
using Application.Manufacturing;
using Persistence;

namespace Application.WorkEfforts;

public class IssueProductionRunReservations
{
    public class Command : IRequest<Result<Unit>>
    {
        public IssueProductionRunReservationsParams Params { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly IProductionRunService _productionRunService;
        private readonly DataContext _context;

        public Handler(IProductionRunService productionRunService, DataContext context)
        {
            _productionRunService = productionRunService;
            _context = context;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

                await _productionRunService.IssueProductionRunReservations(
                    request.Params.WorkEffortId,
                    request.Params.FailIfNotEnoughQoh,
                    request.Params.ReasonEnumId,
                    request.Params.Description
                );

                await _context.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);

                return Result<Unit>.Success(Unit.Value);
            }
            catch (Exception ex)
            {
                return Result<Unit>.Failure($"Error issuing reservations: {ex.Message}");
            }
        }
    }
}