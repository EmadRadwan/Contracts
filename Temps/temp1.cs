var partyGlAccounts = await (
    from pga in _context.PartyGlAccounts
    join a in _context.GlAccounts on pga.GlAccountId equals a.GlAccountId                  // child account
    join parent in _context.GlAccounts on a.ParentGlAccountId equals parent.GlAccountId   // ← added: parent account
    join gat in _context.GlAccountTypes on pga.GlAccountTypeId equals gat.GlAccountTypeId
    join p in _context.Parties on pga.PartyId equals p.PartyId
    join role in _context.RoleTypes on pga.RoleTypeId equals role.RoleTypeId
    where pga.OrganizationPartyId == request.CompanyId
    select new GetPartyGlAccountDto
    {
        PartyId              = p.PartyId,
        PartyDescription     = p.Description,  // or p.Name / whatever your field is
        GlAccountId          = pga.GlAccountId,
        RoleTypeId           = pga.RoleTypeId,
        RoleDescription      = role.Description ?? role.RoleTypeId,
        GlAccountTypeDescription = gat.Description,
        GlAccountName        = pga.GlAccountId + " - " + a.AccountNameArabic,

        // ── NEW fields ──
        ParentGlAccountId    = a.ParentGlAccountId,
        ParentGlAccountName  = parent.AccountNameArabic ?? parent.AccountName ?? "غير محدد",
        FullGlAccountPath    = parent.AccountNameArabic + " → " + a.AccountNameArabic   // optional: nice for display
    }).ToListAsync(cancellationToken);