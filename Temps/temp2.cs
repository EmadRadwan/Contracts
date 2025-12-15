// REFACTOR: Replaced rich Arabic DueStatusArabic with minimal translatable fields
// Reason: string.Format and interpolation cannot be translated to SQL → breaks OData $filter
// Instead, we project raw values needed to compute status on client-side:
// - DaysOverdue (negative = future, 0 = today, positive = past)
// - IsDisbursement flag
// This enables server-side filtering on date logic if needed, while rich text is added in UI
DueStatusArabic = null, // Remove entirely or keep as placeholder

// New translatable fields for client computation
DaysUntilDue = EF.Functions.DateDiffDay(Today, (DateTime)pyt.EffectiveDate), // Positive = future, 0 = today, negative = overdue
IsDisbursement = ptt.ParentTypeId == "DISBURSEMENT"