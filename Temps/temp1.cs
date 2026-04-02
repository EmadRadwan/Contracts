var finalQuery = await query.ToListAsync(cancellationToken);

foreach (var record in finalQuery)
{
    // Calculate DaysUntilDue in memory (safe and reliable)
    record.DaysUntilDue = (record.EffectiveDate - Today).Days;

    // Then your existing DueStatusArabic logic...
    var date = record.EffectiveDate;
    // ... quarter calculation ...

    if (record.StatusId == "PMNT_NOT_PAID")
    {
        // your existing logic using record.DaysUntilDue
        ...
    }
    else
    {
        record.DueStatusArabic = record.StatusDescription;
    }
}

return finalQuery.AsQueryable();