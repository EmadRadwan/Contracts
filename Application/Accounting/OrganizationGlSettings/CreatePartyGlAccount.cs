using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.OrganizationGlSettings;

public class CreatePartyGlAccountCommand : IRequest<Results<CreatePartyGlAccountResponse>>
{
    public string CompanyId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public string GlAccountId { get; set; } = null!;
    public string GlAccountTypeId { get; set; } = null!;
}

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
        var exists = await _context.PartyGlAccounts
            .AnyAsync(p =>
                    p.OrganizationPartyId == request.CompanyId &&
                    p.PartyId == request.PartyId &&
                    p.RoleTypeId == request.RoleTypeId &&
                    p.GlAccountTypeId == request.GlAccountTypeId &&
                    p.GlAccountId == request.GlAccountId, // ← added
                ct);

        if (exists)
        {
            return Results<CreatePartyGlAccountResponse>.Failure(
                "PARTY_GL_ACCOUNT_ALREADY_EXISTS"
            );
        }

        var record = new PartyGlAccount
        {
            OrganizationPartyId = request.CompanyId,
            PartyId = request.PartyId,
            RoleTypeId = request.RoleTypeId,
            GlAccountId = request.GlAccountId,
            GlAccountTypeId = request.GlAccountTypeId,
            CreatedStamp = DateTime.UtcNow,
            CreatedTxStamp = DateTime.UtcNow,
        };

        _context.PartyGlAccounts.Add(record);
        var saved = await _context.SaveChangesAsync(ct) > 0;

        if (!saved)
            return Results<CreatePartyGlAccountResponse>.Failure("Failed to save record");

        return Results<CreatePartyGlAccountResponse>.Success(new CreatePartyGlAccountResponse
        {
            PartyGlAccountId = record.OrganizationPartyId
        });
    }
}

public class CreatePartyGlAccountResponse
{
    public string? PartyGlAccountId { get; set; }
    // add more fields if frontend needs them
}