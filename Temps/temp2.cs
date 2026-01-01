if (!context.GlAccountOrganizations.Any())
{
    // 1. Load and seed default GlAccountOrganization records
    var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/gl_account_organization.json");
    var jsonData = File.ReadAllText(path);
    var defaultGlAccountOrganizations = JsonConvert.DeserializeObject<List<GlAccountOrganization>>(jsonData);
    await context.GlAccountOrganizations.AddRangeAsync(defaultGlAccountOrganizations);
    await context.SaveChangesAsync();

    // 2. Create sub-accounts for SUPPLIER/CONTRACTOR and CUSTOMER parties
    const string OrganizationPartyId = "Company";
    var now = DateTime.Now;
    var txNow = now.AddSeconds(-5);

    // Define the two parent accounts
    var payableParent = await context.GlAccounts
        .AsNoTracking()
        .FirstOrDefaultAsync(a => a.GlAccountId == "210000");

    var receivableParent = await context.GlAccounts
        .AsNoTracking()
        .FirstOrDefaultAsync(a => a.GlAccountId == "121100");

    if (payableParent == null) throw new Exception("Parent GL Account 210000 (Accounts Payable) not found.");
    if (receivableParent == null) throw new Exception("Parent GL Account 121100 (Accounts Receivable) not found.");

    // Load all existing child GL_ACCOUNT_IDs for both parents
    var existingPayableIds = await context.GlAccounts
        .Where(a => a.GlAccountId.StartsWith("210") && a.GlAccountId.Length > 6)
        .Select(a => a.GlAccountId)
        .ToHashSetAsync();

    var existingReceivableIds = await context.GlAccounts
        .Where(a => a.GlAccountId.StartsWith("1211") && a.GlAccountId.Length > 6)
        .Select(a => a.GlAccountId)
        .ToHashSetAsync();

    // Helper to generate unique child ID for a given prefix (210 or 1211)
    string GenerateNextId(string prefix, HashSet<string> existingSet)
    {
        int seq = 1;
        while (true)
        {
            string candidate = $"{prefix}{seq:D3}"; // 210001, 121101, etc.
            if (!existingSet.Contains(candidate))
            {
                existingSet.Add(candidate);
                return candidate;
            }
            seq++;
        }
    }

    // Get all relevant parties
    var targetParties = await context.Parties
        .Where(p => p.MainRole == "CONTRACTOR" || 
                    p.MainRole == "SUPPLIER" || 
                    p.MainRole == "CUSTOMER")
        .Select(p => new { p.PartyId, p.Description, p.MainRole })
        .ToListAsync();

    var newGlAccounts = new List<GlAccount>();
    var newGlAccountOrgs = new List<GlAccountOrganization>();

    foreach (var party in targetParties)
    {
        bool isCustomer = party.MainRole == "CUSTOMER";
        var parent = isCustomer ? receivableParent : payableParent;
        var existingSet = isCustomer ? existingReceivableIds : existingPayableIds;
        string prefix = isCustomer ? "1211" : "210";

        string newGlId = GenerateNextId(prefix, existingSet);
        string partyName = party.Description ?? $"Party {party.PartyId}";

        string accountName = isCustomer 
            ? $"ACCOUNTS RECEIVABLE - {partyName}"
            : $"ACCOUNTS PAYABLE - {partyName}";

        string accountNameArabic = isCustomer 
            ? $"مدينون - {partyName}"
            : $"الدائنون - {partyName}";

        // Create new GlAccount
        var newGlAccount = new GlAccount
        {
            GlAccountId = newGlId,
            GlAccountTypeId = parent.GlAccountTypeId,       // ACCOUNTS_PAYABLE or ACCOUNTS_RECEIVABLE
            GlAccountClassId = parent.GlAccountClassId,     // CURRENT_LIABILITY or CURRENT_ASSET
            GlResourceTypeId = parent.GlResourceTypeId,     // MONEY
            GlXbrlClassId = parent.GlXbrlClassId,
            ParentGlAccountId = parent.GlAccountId,         // 210000 or 121100
            AccountCode = newGlId,
            AccountName = accountName,
            AccountNameArabic = accountNameArabic,
            Description = null,
            ProductId = null,
            ExternalId = party.PartyId,                     // Link back to party
            LastUpdatedStamp = now,
            LastUpdatedTxStamp = txNow,
            CreatedStamp = now,
            CreatedTxStamp = txNow
        };

        newGlAccounts.Add(newGlAccount);

        // Create GlAccountOrganization for Company
        var newGlOrg = new GlAccountOrganization
        {
            GlAccountId = newGlId,
            OrganizationPartyId = OrganizationPartyId,
            RoleTypeId = null,
            FromDate = new DateTime(2001, 1, 1),
            ThruDate = null,
            LastUpdatedStamp = now,
            LastUpdatedTxStamp = txNow,
            CreatedStamp = now,
            CreatedTxStamp = txNow
        };

        newGlAccountOrgs.Add(newGlOrg);
    }

    // Bulk insert if any new accounts were created
    if (newGlAccounts.Any())
    {
        await context.GlAccounts.AddRangeAsync(newGlAccounts);
        await context.GlAccountOrganizations.AddRangeAsync(newGlAccountOrgs);
        await context.SaveChangesAsync();
    }
}