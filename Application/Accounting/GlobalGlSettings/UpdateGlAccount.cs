using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.GlobalGlSettings;

public class UpdateGlAccount
{
    public class Command : IRequest<Results<UpdateGlAccountResponse>>
    {
        public UpdateGlAccountRequest? Request { get; set; }
    }

    public class Handler : IRequestHandler<Command, Results<UpdateGlAccountResponse>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _context = context;
            _userAccessor = userAccessor;
        }

        public async Task<Results<UpdateGlAccountResponse>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var dto = request.Request!;

            if (string.IsNullOrWhiteSpace(dto.GlAccountId))
                return Results<UpdateGlAccountResponse>.Failure("GlAccountId is required", "GL_ACCOUNT_ID_REQUIRED");

            var currentUsername = _userAccessor.GetUsername();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == currentUsername, cancellationToken);

            if (user == null)
                return Results<UpdateGlAccountResponse>.Failure("Unauthorized: User not found", "USER_NOT_FOUND");

            try
            {
                var glAccount = await _context.GlAccounts
                    .FirstOrDefaultAsync(a => a.GlAccountId == dto.GlAccountId, cancellationToken);

                if (glAccount == null)
                    return Results<UpdateGlAccountResponse>.Failure("GL Account not found", "GL_ACCOUNT_NOT_FOUND");

                if (glAccount?.GlAccountId == dto.ParentGlAccountId)
                    return Results<UpdateGlAccountResponse>.Failure("Cannot assign account as child to itself", "GL_ACCOUNT_INVALID_PARENT");

                // Update only the allowed fields
                glAccount.AccountNameArabic = dto.AccountName?.Trim();
                glAccount.Description = dto.Description?.Trim();
                glAccount.ParentGlAccountId = dto.ParentGlAccountId;
                glAccount.LastUpdatedStamp = DateTime.UtcNow;

                var saved = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!saved)
                    return Results<UpdateGlAccountResponse>.Failure("Failed to update GL account", "GL_ACCOUNT_UPDATE_FAILED");

                var response = new UpdateGlAccountResponse
                {
                    GlAccountId = glAccount.GlAccountId,
                    AccountCode = glAccount.AccountCode,
                    AccountName = glAccount.AccountNameArabic,
                    Description = glAccount.Description,
                    GlAccountTypeId = glAccount.GlAccountTypeId,
                    GlAccountClassId = glAccount.GlAccountClassId,
                    GlResourceTypeId = glAccount.GlResourceTypeId,
                    ParentGlAccountId = glAccount.ParentGlAccountId,
                    LastUpdatedDate = glAccount.LastUpdatedStamp
                };

                return Results<UpdateGlAccountResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return Results<UpdateGlAccountResponse>.Failure($"Failed to update GL account: {ex.Message}", "GL_ACCOUNT_UPDATE_FAILED");
            }
        }
    }
}

public class UpdateGlAccountRequest
{
    public string? GlAccountId { get; set; }
    public string? AccountName { get; init; }
    public string? Description { get; init; }
    public string? ParentGlAccountId { get; init; }
}

public class UpdateGlAccountResponse
{
    public string? GlAccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public string? Description { get; set; }
    public string? GlAccountTypeId { get; set; }
    public string? GlAccountClassId { get; set; }
    public string? GlResourceTypeId { get; set; }
    public string? ParentGlAccountId { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
}
