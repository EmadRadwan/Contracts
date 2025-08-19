using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Transactions
{
    public class CreateAcctgTransEntry
    {
        public class Command : IRequest<Result<AcctgTransEntry>>
        {
            public AcctgTransEntry AcctgTransEntry { get; set; }
        }

        public class CreateAcctgTransEntryValidator : AbstractValidator<AcctgTransEntry>
        {
            public CreateAcctgTransEntryValidator()
            {
                RuleFor(x => x.AcctgTransId)
                    .NotEmpty()
                    .WithMessage("Accounting transaction ID is required.");

                RuleFor(x => x.Amount)
                    .GreaterThanOrEqualTo(0)
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
                RuleFor(x => x.AcctgTransEntry).SetValidator(new CreateAcctgTransEntryValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<AcctgTransEntry>>
        {
            private readonly DataContext _context;
            private readonly IAcctgTransService _acctgTransService;

            public Handler(DataContext context, IAcctgTransService acctgTransService)
            {
                _context = context;
                _acctgTransService = acctgTransService;
            }

            public async Task<Result<AcctgTransEntry>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // get glAccountTypeId
                    var glAccountTypeId = await _context.GlAccounts
                        .Where(x => x.GlAccountId == request.AcctgTransEntry.GlAccountId)
                        .Select(x => x.GlAccountTypeId)
                          .FirstOrDefaultAsync(cancellationToken);
                    
                    request.AcctgTransEntry.GlAccountTypeId = glAccountTypeId;
                    // Call the CreateAcctgTransEntry function in the service
                    var entry = await _acctgTransService.CreateAcctgTransEntry(request.AcctgTransEntry);

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return Result<AcctgTransEntry>.Success(entry);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<AcctgTransEntry>.Failure("Error creating accounting transaction entry");
                }
            }
        }
    }
}