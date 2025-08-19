using Application.Accounting.Services;



using MediatR;
using Persistence;

namespace Application.Accounting.OrganizationGlSettings;

public class CloseFinancialTimePeriod
{
    // 1) The Command holds the ID or any other parameters needed
    public class Command : IRequest<Result<CloseFinancialTimePeriodResult>>
    {
        public string CustomTimePeriodId { get; set; }
    }

    // 2) The Handler does the orchestration: calls the service, handles errors,
    //    and returns a Result<CloseFinancialTimePeriodResult>.
    public class Handler : IRequestHandler<Command, Result<CloseFinancialTimePeriodResult>>
    {
        private readonly IGeneralLedgerService _generalLedgerService;
        private readonly DataContext _context;

        public Handler(IGeneralLedgerService generalLedgerService, DataContext context)
        {
            _context = context;
            _generalLedgerService = generalLedgerService;
        }

        public async Task<Result<CloseFinancialTimePeriodResult>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            // We'll create a transaction if we want to ensure that
            // multiple DB writes occur atomically.
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // 1) Call the existing method that closes the time period
                //    (verifying no open children, creating period-closing entries, etc.)
                await _generalLedgerService.CloseFinancialTimePeriod(request.CustomTimePeriodId);

                // 2) If everything is okay, commit the transaction
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // 3) Build a success result object
                var response = new CloseFinancialTimePeriodResult
                {
                    Message = $"Time period '{request.CustomTimePeriodId}' has been closed successfully."
                };

                return Result<CloseFinancialTimePeriodResult>.Success(response);
            }
            catch (Exception ex)
            {
                // 4) Roll back on any error
                await transaction.RollbackAsync(cancellationToken);

                // Return a failure with the exception message
                return Result<CloseFinancialTimePeriodResult>.Failure(
                    $"Error closing financial time period: {ex.Message}");
            }
        }
    }
}