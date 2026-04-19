public async Task<IQueryable<PaymentRecord>> Handle(...) 
{
    // ... build query ...

    var finalList = await query.ToListAsync(cancellationToken);
    return finalList.AsQueryable();   // or just return query if you don't need post-processing
}