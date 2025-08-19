using API.Controllers.Accounting.Transactions;
using Application.Core;
using FluentValidation;
using MediatR;
using Persistence;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Accounting.Services;

namespace Application.Accounting.Transactions
{
    public class PostAcctgTrans
    {
        public class Command : IRequest<Result<List<string>>>
        {
            public string AcctgTransId { get; set; }
            public bool VerifyOnly { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.AcctgTransId)
                    .NotEmpty()
                    .WithMessage("Accounting transaction ID is required.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<List<string>>>
        {
            private readonly DataContext _context;
            private readonly IGeneralLedgerService _generalLedgerService;

            public Handler(DataContext context, IGeneralLedgerService generalLedgerService)
            {
                _context = context;
                _generalLedgerService = generalLedgerService;
            }

            public async Task<Result<List<string>>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // REFACTORED: Call existing PostAcctgTrans function
                    var messages = await _generalLedgerService.PostAcctgTrans(request.AcctgTransId, request.VerifyOnly);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return Result<List<string>>.Success(messages);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<List<string>>.Failure($"Error posting accounting transaction: {ex.Message}");
                }
            }
        }
    }
}