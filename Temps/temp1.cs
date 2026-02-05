public class Handler : IRequestHandler<CreatePartyGlAccountCommand, Results<CreatePartyGlAccountResponse>>
{
    private readonly DataContext _context;
    private readonly IUserAccessor _userAccessor;

    public Handler(DataContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<Results<CreatePartyGlAccountResponse>> Handle(
        CreatePartyGlAccountCommand request,
        CancellationToken ct)
    {
        // 1. Check if the PartyRole already exists for this Party + RoleType
        var roleExists = await _context.PartyRoles
            .AnyAsync(pr =>
                pr.PartyId == request.PartyId &&
                pr.RoleTypeId == request.RoleTypeId,
                ct);

        // 2. If not, create the PartyRole first
        if (!roleExists)
        {
            var newRole = new PartyRole
            {
                PartyId           = request.PartyId,
                RoleTypeId        = request.RoleTypeId,
                CreatedStamp      = DateTime.UtcNow,
                CreatedTxStamp    = DateTime.UtcNow,
                LastUpdatedStamp  = DateTime.UtcNow,
                LastUpdatedTxStamp = DateTime.UtcNow
            };

            _context.PartyRoles.Add(newRole);

            // Optional: you can log or return early if role creation fails,
            // but usually we just continue and let the final SaveChanges fail if needed
        }

        // 3. Now check if the specific PartyGlAccount combination already exists
        var glExists = await _context.PartyGlAccounts
            .AnyAsync(p =>
                p.OrganizationPartyId == request.CompanyId &&
                p.PartyId             == request.PartyId &&
                p.RoleTypeId          == request.RoleTypeId &&
                p.GlAccountId         == request.GlAccountId &&
                p.GlAccountTypeId     == request.GlAccountTypeId,
                ct);

        if (glExists)
        {
            return Results<CreatePartyGlAccountResponse>.Failure("PARTY_GL_ACCOUNT_ALREADY_EXISTS");
        }

        // 4. Create the new PartyGlAccount record
        var record = new PartyGlAccount
        {
            OrganizationPartyId = request.CompanyId,
            PartyId             = request.PartyId,
            RoleTypeId          = request.RoleTypeId,
            GlAccountId         = request.GlAccountId,
            GlAccountTypeId     = request.GlAccountTypeId,

            CreatedStamp        = DateTime.UtcNow,
            CreatedTxStamp      = DateTime.UtcNow,
            LastUpdatedStamp    = DateTime.UtcNow,
            LastUpdatedTxStamp  = DateTime.UtcNow,
        };

        _context.PartyGlAccounts.Add(record);

        // 5. Save everything in one transaction
        var saved = await _context.SaveChangesAsync(ct) > 0;

        if (!saved)
        {
            return Results<CreatePartyGlAccountResponse>.Failure("Failed to save record");
        }

        return Results<CreatePartyGlAccountResponse>.Success(new CreatePartyGlAccountResponse
        {
            PartyGlAccountId = $"{request.CompanyId}-{request.PartyId}-{request.RoleTypeId}-{request.GlAccountId}"
            // ↑ better composite key representation – adjust as needed
        });
    }
}