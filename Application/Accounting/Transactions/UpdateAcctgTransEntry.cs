using API.Controllers.Accounting.Transactions;
using Application.Accounting.Services;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Accounting.Transactions
{
    public class UpdateAcctgTransEntry
    {
        public class Command : IRequest<Result<AcctgTransEntryDto>>
        {
            public AcctgTransEntry AcctgTransEntry { get; set; }
        }

        public class UpdateAcctgTransEntryValidator : AbstractValidator<AcctgTransEntry> // Changed to AcctgTransEntry
        {
            public UpdateAcctgTransEntryValidator()
            {
                RuleFor(x => x.AcctgTransId)
                    .NotEmpty()
                    .WithMessage("Accounting transaction ID is required.");

                RuleFor(x => x.AcctgTransEntrySeqId)
                    .NotEmpty()
                    .WithMessage("Accounting transaction entry sequence ID is required.");

                RuleFor(x => x.Amount)
                    .GreaterThanOrEqualTo(0)
                    .When(x => x.Amount.HasValue)
                    .WithMessage("Amount must be non-negative.");

                RuleFor(x => x.DebitCreditFlag)
                    .Must(flag => flag == "D" || flag == "C")
                    .When(x => x.DebitCreditFlag != null)
                    .WithMessage("DebitCreditFlag must be either 'D' or 'C'.");
            }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.AcctgTransEntry).SetValidator(new UpdateAcctgTransEntryValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<AcctgTransEntryDto>>
        {
            private readonly DataContext _context;
            private readonly IAcctgTransService _acctgTransService;

            public Handler(DataContext context, IAcctgTransService acctgTransService)
            {
                _context = context;
                _acctgTransService = acctgTransService;
            }

            public async Task<Result<AcctgTransEntryDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var entry = await _acctgTransService.UpdateAcctgTransEntry(request.AcctgTransEntry);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return Result<AcctgTransEntryDto>.Success(entry);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<AcctgTransEntryDto>.Failure($"Error updating accounting transaction entry: {ex.Message}");
                }
            }
        }
    }
}