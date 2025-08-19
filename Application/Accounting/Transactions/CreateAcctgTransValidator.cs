using Application.Accounting.Services.Models;
using FluentValidation;

namespace Application.Accounting.Transactions;

public class CreateAcctgTransValidator : AbstractValidator<CreateQuickAcctgTransAndEntriesParams>
{
    public CreateAcctgTransValidator()
    {
        RuleFor(x => x.TransactionDate).NotEmpty();
    }
}