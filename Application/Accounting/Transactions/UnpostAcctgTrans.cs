using Application.Core;
using FluentValidation;
using MediatR;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Application.Accounting.Transactions
{
    public class UnpostAcctgTrans
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string AcctgTransId { get; set; }
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

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var acctgTrans = await _context.AcctgTrans.FindAsync(new object[] { request.AcctgTransId }, cancellationToken);

                if (acctgTrans == null)
                {
                    return Result<Unit>.Failure("Accounting transaction not found.");
                }

                if (acctgTrans.IsPosted != "Y")
                {
                    return Result<Unit>.Failure("Accounting transaction is not posted.");
                }

                acctgTrans.IsPosted = "N";
                acctgTrans.PostedDate = null;

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result) return Result<Unit>.Failure("Failed to un-post accounting transaction.");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
