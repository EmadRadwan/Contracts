using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Accounting.Transactions
{
    public class CreateAcctgTrans
    {
        public class Command : IRequest<Result<CreateAcctgTransResult>>
        {
            public CreateAcctgTransParams CreateAcctgTransParams { get; set; }
        }
        
        public class CreateAcctgTransValidator : AbstractValidator<CreateAcctgTransParams>
        {
            public CreateAcctgTransValidator()
            {
                // Validate that PostedDate, if provided, is not in the future
                RuleFor(x => x.PostedDate)
                    .LessThanOrEqualTo(DateTime.UtcNow)
                    .When(x => x.PostedDate.HasValue)
                    .WithMessage("Posted date cannot be in the future.");
            }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                // Ensure you have a validator for CreateAcctgTransParams (e.g., CreateAcctgTransValidator)
                RuleFor(x => x.CreateAcctgTransParams).SetValidator(new CreateAcctgTransValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<CreateAcctgTransResult>>
        {
            private readonly DataContext _context;
            private readonly IAcctgTransService _acctgTransService;

            public Handler(DataContext context, IAcctgTransService acctgTransService)
            {
                _context = context;
                _acctgTransService = acctgTransService;
            }

            public async Task<Result<CreateAcctgTransResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                // Begin a transaction
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Call the updated CreateAcctgTrans function in your service
                    var acctgTransId = await _acctgTransService.CreateAcctgTrans(request.CreateAcctgTransParams);
                    
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    var result = new CreateAcctgTransResult
                    {
                        AcctgTransId = acctgTransId,
                        // You can add additional return values here if needed.
                    };

                    return Result<CreateAcctgTransResult>.Success(result);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<CreateAcctgTransResult>.Failure("Error creating accounting transaction");
                }
            }
        }
    }
}
