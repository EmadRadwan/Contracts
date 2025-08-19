using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Persistence;

namespace Application.Accounting.Transactions
{
    public class UpdateAcctgTrans
    {
        public class Command : IRequest<Result<UpdateAcctgTransResult>>
        {
            public AcctgTran AcctgTransParams { get; set; }
        }

        public class UpdateAcctgTransValidator : AbstractValidator<AcctgTran>
        {
            public UpdateAcctgTransValidator()
            {
                // Validate that AcctgTransId is provided (required since it's the PK)
                RuleFor(x => x.AcctgTransId)
                    .NotEmpty()
                    .WithMessage("Accounting transaction ID is required.");

                // Validate that PostedDate, if provided, is not in the future
                RuleFor(x => x.PostedDate)
                    .LessThanOrEqualTo(DateTime.UtcNow)
                    .When(x => x.PostedDate.HasValue)
                    .WithMessage("Posted date cannot be in the future.");

                // Optional: Add more validation rules for other fields if needed
                // Example: Ensure AcctgTransTypeId is valid if provided
                RuleFor(x => x.AcctgTransTypeId)
                    .NotEmpty()
                    .When(x => x.AcctgTransTypeId != null)
                    .WithMessage("Accounting transaction type ID cannot be empty.");
            }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                // Ensure the AcctgTransParams is validated using UpdateAcctgTransValidator
                RuleFor(x => x.AcctgTransParams).SetValidator(new UpdateAcctgTransValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<UpdateAcctgTransResult>>
        {
            private readonly DataContext _context;
            private readonly IAcctgTransService _acctgTransService;

            public Handler(DataContext context, IAcctgTransService acctgTransService)
            {
                _context = context;
                _acctgTransService = acctgTransService;
            }

            public async Task<Result<UpdateAcctgTransResult>> Handle(Command request,
                CancellationToken cancellationToken)
            {
                // Begin a transaction
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Call the UpdateAcctgTrans function in the service
                    var messages = await _acctgTransService.UpdateAcctgTrans(request.AcctgTransParams);

                    // Check if there are any error messages
                    if (messages.Any())
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<UpdateAcctgTransResult>.Failure(string.Join("; ", messages));
                    }

                    // Save changes to the database
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    // Return success with the updated AcctgTransId
                    var result = new UpdateAcctgTransResult
                    {
                        AcctgTransId = request.AcctgTransParams.AcctgTransId
                    };

                    return Result<UpdateAcctgTransResult>.Success(result);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<UpdateAcctgTransResult>.Failure("Error updating accounting transaction");
                }
            }
        }

    }
}