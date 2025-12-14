using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.GlobalGlSettings;

public class CreateGlAccount
{
    public class Command : IRequest<Results<CreateGlAccountResponse>>
    {
        public CreateGlAccountRequest? Request { get; set; }
    }

    public class Handler : IRequestHandler<Command, Results<CreateGlAccountResponse>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _context = context;
            _userAccessor = userAccessor;
        }

        public async Task<Results<CreateGlAccountResponse>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var dto = request.Request!;

            var currentUsername = _userAccessor.GetUsername();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == currentUsername, cancellationToken);

            if (user == null)
                return Results<CreateGlAccountResponse>.Failure("Unauthorized: User not found", "USER_NOT_FOUND");

            var now = DateTime.UtcNow;

            try
            {
                // Generate the next account code based on parent's children
                var newAccountCode = await GenerateNextAccountCode(dto.ParentGlAccountId, cancellationToken);

                if (newAccountCode == null)
                    return Results<CreateGlAccountResponse>.Failure("Failed to generate a unique account code", "ACCOUNT_CODE_GENERATION_FAILED");

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
                    return Results<CreateGlAccountResponse>.Failure("Failed to save GL account", "GL_ACCOUNT_SAVE_FAILED");

                // Build response
                var response = new CreateGlAccountResponse
                {
                    GlAccountId = glAccount.GlAccountId,
                    AccountCode = glAccount.AccountCode,
                    AccountName = glAccount.AccountNameArabic,
                    Description = glAccount.Description,
                    GlAccountTypeId = glAccount.GlAccountTypeId,
                    GlAccountClassId = glAccount.GlAccountClassId,
                    GlResourceTypeId = glAccount.GlResourceTypeId,
                    ParentGlAccountId = glAccount.ParentGlAccountId,
                    CreatedDate = glAccount.CreatedStamp
                };

                return Results<CreateGlAccountResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return Results<CreateGlAccountResponse>.Failure($"Failed to create GL account: {ex.Message}", "GL_ACCOUNT_CREATE_FAILED");
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

                // Determine the hierarchy level based on parent code pattern
                // Level 1: 100000, 200000... (increment 100000)
                // Level 2: 110000, 120000... (increment 10000)
                // Level 3: 111000, 112000... (increment 1000)
                // Level 4: 111100, 111200... (increment 100)
                // Level 5: 111110, 111120... (increment 10)
                // Level 6: 111111, 111112... (increment 1)

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

        /// <summary>
        /// Determines the increment for child accounts based on parent code structure.
        /// Analyzes which digit position should increment for children.
        /// </summary>
        private int DetermineChildIncrement(int parentCode)
        {
            // Convert to string to analyze digit positions
            var codeStr = parentCode.ToString().PadLeft(6, '0');

            // Count trailing zeros to determine the level
            // Example: 800000 -> 5 trailing zeros -> children increment by 10000
            // Example: 810000 -> 4 trailing zeros -> children increment by 1000
            // Example: 811000 -> 3 trailing zeros -> children increment by 100
            // Example: 811100 -> 2 trailing zeros -> children increment by 10
            // Example: 811110 -> 1 trailing zero  -> children increment by 1
            // Example: 811111 -> 0 trailing zeros -> children increment by 1

            int trailingZeros = 0;
            for (int i = 5; i >= 0; i--)
            {
                if (codeStr[i] == '0')
                    trailingZeros++;
                else
                    break;
            }

            // Children increment at one level deeper
            // 5 trailing zeros -> 10000 (10^4)
            // 4 trailing zeros -> 1000  (10^3)
            // 3 trailing zeros -> 100   (10^2)
            // 2 trailing zeros -> 10    (10^1)
            // 1 trailing zero  -> 1     (10^0)
            // 0 trailing zeros -> 1     (10^0)
            if (trailingZeros == 0)
                return 1;

            return (int)Math.Pow(10, trailingZeros - 1);
        }
    }
}

public class CreateGlAccountRequest
{
    public string? AccountName { get; init; }
    public string? GlResourceTypeId { get; init; }
    public string? GlAccountTypeId { get; init; }
    
    public string? GlAccountClassId { get; init; }
    public string? ParentGlAccountId { get; init; }
    public string? Description { get; init; }
}

public class CreateGlAccountResponse
{
    public string? GlAccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public string? Description { get; set; }
    public string? GlAccountTypeId { get; set; }
    public string? GlAccountClassId { get; set; }
    public string? GlResourceTypeId { get; set; }
    public string? ParentGlAccountId { get; set; }
    public DateTime? CreatedDate { get; set; }
}
