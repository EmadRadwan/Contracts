using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Accounting.Transactions
{
    public class DeleteAcctgTransEntry
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string AcctgTransId { get; set; }
            public string AcctgTransEntrySeqId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.AcctgTransId)
                    .NotEmpty()
                    .WithMessage("Accounting transaction ID is required.");

                RuleFor(x => x.AcctgTransEntrySeqId)
                    .NotEmpty()
                    .WithMessage("Accounting transaction entry sequence ID is required.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IAcctgTransService _acctgTransService;

            public Handler(DataContext context, IAcctgTransService acctgTransService)
            {
                _context = context;
                _acctgTransService = acctgTransService;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    await _acctgTransService.DeleteAcctgTransEntry(request.AcctgTransId, request.AcctgTransEntrySeqId);

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<Unit>.Failure($"Error deleting accounting transaction entry: {ex.Message}");
                }
            }
        }
    }
}