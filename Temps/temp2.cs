// REFACTOR: Now updates GlAccountTypeId, GlAccountClassId, and GlResourceTypeId if provided.
// Also fetches and returns Arabic descriptions from reference tables to support virtual ComboBox binding after save.
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

            if (glAccount.GlAccountId == dto.ParentGlAccountId)
                return Results<UpdateGlAccountResponse>.Failure("Cannot assign account as child to itself", "GL_ACCOUNT_INVALID_PARENT");

            // Update allowed fields
            glAccount.AccountNameArabic = dto.AccountName?.Trim();
            glAccount.Description = dto.Description?.Trim();
            glAccount.ParentGlAccountId = dto.ParentGlAccountId;

            // REFACTOR: Now update the three dropdown fields
            if (!string.IsNullOrWhiteSpace(dto.GlAccountTypeId))
                glAccount.GlAccountTypeId = dto.GlAccountTypeId;

            if (!string.IsNullOrWhiteSpace(dto.GlAccountClassId))
                glAccount.GlAccountClassId = dto.GlAccountClassId;

            if (!string.IsNullOrWhiteSpace(dto.GlResourceTypeId))
                glAccount.GlResourceTypeId = dto.GlResourceTypeId;

            glAccount.LastUpdatedStamp = DateTime.UtcNow;

            var saved = await _context.SaveChangesAsync(cancellationToken) > 0;
            if (!saved)
                return Results<UpdateGlAccountResponse>.Failure("Failed to update GL account", "GL_ACCOUNT_UPDATE_FAILED");

            // Fetch Arabic descriptions for binding
            var typeDesc = await _context.GlAccountTypes
                .Where(t => t.GlAccountTypeId == glAccount.GlAccountTypeId)
                .Select(t => t.DescriptionArabic)
                .FirstOrDefaultAsync(cancellationToken);

            var classDesc = await _context.GlAccountClasses
                .Where(c => c.GlAccountClassId == glAccount.GlAccountClassId)
                .Select(c => c.DescriptionArabic)
                .FirstOrDefaultAsync(cancellationToken);

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
                LastUpdatedDate = glAccount.LastUpdatedStamp,
                GlAccountTypeDescriptionArabic = typeDesc,
                GlAccountClassDescriptionArabic = classDesc
            };

            return Results<UpdateGlAccountResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Results<UpdateGlAccountResponse>.Failure($"Failed to update GL account: {ex.Message}", "GL_ACCOUNT_UPDATE_FAILED");
        }
    }
}