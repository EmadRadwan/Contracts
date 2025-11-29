private async Task<UserDto> CreateUserObject(AppUserLogin user)
{
    var roles = await _userManager.GetRolesAsync(user);

    // REFACTOR: Query the Parties table directly to get the name
    // This avoids lazy loading and works even if navigation properties are not included
    string organizationPartyName = string.Empty;

    if (user.OrganizationPartyId.HasValue)
    {
        var party = await _context.Parties
            .Where(p => p.Id == user.OrganizationPartyId.Value)
            .Select(p => p.PartyName)           // Only select the column we need
            .FirstOrDefaultAsync();

        organizationPartyName = party ?? string.Empty;
    }

    return new UserDto
    {
        Id                    = user.Id,
        DisplayName           = user.DisplayName,
        Image                 = null, // user?.Files?.FirstOrDefault(x => x.IsMain)?.Url,
        Token                 = await _tokenService.CreateToken(user),
        Username              = user.UserName,
        OrganizationPartyId   = user.OrganizationPartyId,
        
        // NEW: Return the actual organization/party name from the Parties table
        OrganizationPartyName = organizationPartyName,

        DualLanguage          = user.DualLanguage,
        Roles                 = roles.ToArray()
    };
}