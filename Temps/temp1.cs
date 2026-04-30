public async Task<OrderRole> CreateOrderRole(
    string orderId, 
    string roleTypeId, 
    string partyId)
{
    var stamp = DateTime.UtcNow;

    // Step 1: Ensure the PartyRole record exists (PartyId + RoleTypeId combination)
    var partyRole = await _context.PartyRoles
        .FirstOrDefaultAsync(pr => pr.PartyId == partyId && pr.RoleTypeId == roleTypeId);

    if (partyRole == null)
    {
        partyRole = new PartyRole
        {
            PartyId = partyId,
            RoleTypeId = roleTypeId,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
            // Add CreatedTxStamp / LastUpdatedTxStamp if your entity has them
        };

        _context.PartyRoles.Add(partyRole);
        await _context.SaveChangesAsync();   // Save PartyRole first
    }

    // Step 2: Now create the OrderRole (with FK to PartyRole)
    var orderRole = new OrderRole
    {
        OrderId = orderId,
        PartyId = partyId,
        RoleTypeId = roleTypeId,
        CreatedStamp = stamp,
        LastUpdatedStamp = stamp
    };

    _context.OrderRoles.Add(orderRole);
    await _context.SaveChangesAsync();

    return orderRole;
}