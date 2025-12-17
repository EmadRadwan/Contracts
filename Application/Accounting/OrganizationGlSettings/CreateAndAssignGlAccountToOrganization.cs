using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.OrganizationGlSettings;

public class CreateAndAssignGlAccountToOrganization
{
    public class Command : IRequest<Results<CreateAndAssignGlAccountResponse>>
    {
        public string CompanyId { get; set; } = null!;
        public CreateAndAssignGlAccountRequest? Request { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("COMPANY_ID must not be empty.");
            RuleFor(x => x.Request)
                .NotNull().WithMessage("Request body must not be null.");
        }
    }

    public class Handler : IRequestHandler<Command, Results<CreateAndAssignGlAccountResponse>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _context = context;
            _userAccessor = userAccessor;
        }

        public async Task<Results<CreateAndAssignGlAccountResponse>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var dto = request.Request!;

            var currentUsername = _userAccessor.GetUsername();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == currentUsername, cancellationToken);

            if (user == null)
                return Results<CreateAndAssignGlAccountResponse>.Failure("Unauthorized: User not found", "USER_NOT_FOUND");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var now = DateTime.UtcNow;

                // Generate the next account code based on parent's children
                var newAccountCode = await GenerateNextAccountCode(dto.ParentGlAccountId, cancellationToken);

                if (newAccountCode == null)
                    return Results<CreateAndAssignGlAccountResponse>.Failure("Failed to generate a unique account code", "ACCOUNT_CODE_GENERATION_FAILED");

                // Create the new GlAccount
                var glAccount = new GlAccount
                {
                    GlAccountId = newAccountCode,
                    AccountCode = newAccountCode,
                    AccountNameArabic = dto.AccountName?.Trim(),
                    Description = dto.Description?.Trim(),
                    GlAccountTypeId = dto.GlAccountTypeId,
                    GlAccountClassId = dto.GlAccountClassId,
                    GlResourceTypeId = dto.GlResourceTypeId,
                    ParentGlAccountId = dto.ParentGlAccountId,
                    CreatedStamp = now,
                    LastUpdatedStamp = now
                };

                _context.GlAccounts.Add(glAccount);

                var saved = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Results<CreateAndAssignGlAccountResponse>.Failure("Failed to save GL account", "GL_ACCOUNT_SAVE_FAILED");
                }

                // Assign the GL account to the organization
                var glAccountOrganization = new GlAccountOrganization
                {
                    GlAccountId = glAccount.GlAccountId,
                    OrganizationPartyId = request.CompanyId,
                    FromDate = now,
                    CreatedStamp = now,
                    CreatedTxStamp = now
                };

                _context.GlAccountOrganizations.Add(glAccountOrganization);

                saved = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Results<CreateAndAssignGlAccountResponse>.Failure("Failed to assign GL account to organization", "GL_ACCOUNT_ASSIGNMENT_FAILED");
                }

                await transaction.CommitAsync(cancellationToken);

                // Build response
                var response = new CreateAndAssignGlAccountResponse
                {
                    GlAccountId = glAccount.GlAccountId,
                    AccountCode = glAccount.AccountCode,
                    AccountName = glAccount.AccountNameArabic,
                    Description = glAccount.Description,
                    GlAccountTypeId = glAccount.GlAccountTypeId,
                    GlAccountClassId = glAccount.GlAccountClassId,
                    GlResourceTypeId = glAccount.GlResourceTypeId,
                    ParentGlAccountId = glAccount.ParentGlAccountId,
                    OrganizationPartyId = request.CompanyId,
                    CreatedDate = glAccount.CreatedStamp
                };

                return Results<CreateAndAssignGlAccountResponse>.Success(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Results<CreateAndAssignGlAccountResponse>.Failure($"Failed to create and assign GL account: {ex.Message}", "GL_ACCOUNT_CREATE_ASSIGN_FAILED");
            }
        }

        private const int MaxCodeValue = 999999;

        private async Task<string?> GenerateNextAccountCode(string? parentGlAccountId, CancellationToken cancellationToken)
        {
            // Get all children of the specified parent account
            var childAccounts = await _context.GlAccounts
                .Where(a => a.ParentGlAccountId == parentGlAccountId)
                .Select(a => a.AccountCode)
                .ToListAsync(cancellationToken);

            int baseCode;
            int increment;

            if (string.IsNullOrEmpty(parentGlAccountId))
            {
                // Top-level account - get all top-level accounts
                var topLevelAccounts = await _context.GlAccounts
                    .Where(a => a.ParentGlAccountId == null)
                    .Select(a => a.AccountCode)
                    .ToListAsync(cancellationToken);

                if (!topLevelAccounts.Any())
                {
                    // First top-level account starts at 100000
                    return "100000";
                }

                // Find the largest code among top-level accounts
                var largestCode = topLevelAccounts
                    .Where(c => !string.IsNullOrEmpty(c) && c.All(char.IsDigit))
                    .Select(c => int.Parse(c))
                    .OrderByDescending(c => c)
                    .FirstOrDefault();

                // Top-level accounts: first digit changes (100000, 200000, 300000...)
                increment = 100000;
                baseCode = largestCode + increment;
            }
            else
            {
                // Child account - base code on parent
                var parentAccount = await _context.GlAccounts
                    .FirstOrDefaultAsync(a => a.GlAccountId == parentGlAccountId, cancellationToken);

                if (parentAccount == null)
                    return null;

                var parentCode = parentAccount.AccountCode ?? parentGlAccountId;

                if (!int.TryParse(parentCode, out var parentCodeNum))
                    return null;

                increment = DetermineChildIncrement(parentCodeNum);

                if (increment == 0)
                    return null; // Cannot create more children at this level

                if (!childAccounts.Any())
                {
                    // First child - add increment to parent code
                    baseCode = parentCodeNum + increment;
                }
                else
                {
                    // Find the largest child code and add increment
                    var largestChildCode = childAccounts
                        .Where(c => !string.IsNullOrEmpty(c) && c.All(char.IsDigit))
                        .Select(c => int.Parse(c))
                        .OrderByDescending(c => c)
                        .FirstOrDefault();

                    baseCode = largestChildCode + increment;
                }
            }

            // Validate code doesn't exceed max value
            if (baseCode > MaxCodeValue)
                return null;

            // Check if the code is unique, try incrementing if not
            var candidateCode = baseCode;

            for (int i = 0; i < 100; i++)
            {
                if (candidateCode > MaxCodeValue)
                    return null;

                var candidateStr = candidateCode.ToString();
                var exists = await _context.GlAccounts
                    .AnyAsync(a => a.GlAccountId == candidateStr || a.AccountCode == candidateStr, cancellationToken);

                if (!exists)
                    return candidateStr;

                candidateCode += increment;
            }

            return null;
        }

        private int DetermineChildIncrement(int parentCode)
        {
            var codeStr = parentCode.ToString().PadLeft(6, '0');

            int trailingZeros = 0;
            for (int i = 5; i >= 0; i--)
            {
                if (codeStr[i] == '0')
                    trailingZeros++;
                else
                    break;
            }

            if (trailingZeros == 0)
                return 1;

            return (int)Math.Pow(10, trailingZeros - 1);
        }
    }
}

public class CreateAndAssignGlAccountRequest
{
    public string? AccountName { get; init; }
    public string? GlResourceTypeId { get; init; }
    public string? GlAccountTypeId { get; init; }
    public string? GlAccountClassId { get; init; }
    public string? ParentGlAccountId { get; init; }
    public string? Description { get; init; }
}

public class CreateAndAssignGlAccountResponse
{
    public string? GlAccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public string? Description { get; set; }
    public string? GlAccountTypeId { get; set; }
    public string? GlAccountClassId { get; set; }
    public string? GlResourceTypeId { get; set; }
    public string? ParentGlAccountId { get; set; }
    public string? OrganizationPartyId { get; set; }
    public DateTime? CreatedDate { get; set; }
}
