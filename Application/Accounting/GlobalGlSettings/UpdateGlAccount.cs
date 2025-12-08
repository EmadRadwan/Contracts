using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.GlobalGlSettings;

public class UpdateGlAccount
{
    public class Command : IRequest<Result<UpdateGlAccountResponse>>
    {
        public UpdateGlAccountRequest? Request { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<UpdateGlAccountResponse>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _context = context;
            _userAccessor = userAccessor;
        }

        public async Task<Result<UpdateGlAccountResponse>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var dto = request.Request!;

            if (string.IsNullOrWhiteSpace(dto.GlAccountId))
                return Result<UpdateGlAccountResponse>.Failure("GlAccountId is required");

            var currentUsername = _userAccessor.GetUsername();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == currentUsername, cancellationToken);

            if (user == null)
                return Result<UpdateGlAccountResponse>.Failure("Unauthorized: User not found");

            try
            {
                var glAccount = await _context.GlAccounts
                    .FirstOrDefaultAsync(a => a.GlAccountId == dto.GlAccountId, cancellationToken);

                if (glAccount == null)
                    return Result<UpdateGlAccountResponse>.Failure("GL Account not found");

                // Update only the allowed fields
                glAccount.AccountNameArabic = dto.AccountName?.Trim();
                glAccount.Description = dto.Description?.Trim();
                glAccount.ParentGlAccountId = dto.ParentGlAccountId;
                glAccount.LastUpdatedStamp = DateTime.UtcNow;

                var saved = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!saved)
                    return Result<UpdateGlAccountResponse>.Failure("Failed to update GL account");

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

                return Result<UpdateGlAccountResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return Result<UpdateGlAccountResponse>.Failure($"Failed to update GL account: {ex.Message}");
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
