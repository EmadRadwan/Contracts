using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Accounting.Transactions
{
    public class CompleteAcctgTransEntries
    {
        public class Command : IRequest<Result<CompleteAcctgTransEntriesResult>>
        {
            public string AcctgTransId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.AcctgTransId)
                    .NotEmpty().WithMessage("AcctgTransId must not be empty.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<CompleteAcctgTransEntriesResult>>
        {
            private readonly DataContext _context;
            private readonly IGeneralLedgerService _generalLedgerService;

            public Handler(DataContext context, IGeneralLedgerService generalLedgerService)
            {
                _context = context;
                _generalLedgerService = generalLedgerService;
            }

            public async Task<Result<CompleteAcctgTransEntriesResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Call the service method that completes the AcctgTransEntries,
                    // mirroring the logic defined in the OFBiz simple-method.
                    await _generalLedgerService.CompleteAcctgTransEntries(request.AcctgTransId);

                    // Persist any changes made during the completion process.
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    var resultObj = new CompleteAcctgTransEntriesResult
                    {
                        AcctgTransId = request.AcctgTransId
                    };

                    return Result<CompleteAcctgTransEntriesResult>.Success(resultObj);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<CompleteAcctgTransEntriesResult>.Failure("Error completing AcctgTransEntries");
                }
            }
        }
    }

    public class CompleteAcctgTransEntriesResult
    {
        public string AcctgTransId { get; set; }
    }
}
