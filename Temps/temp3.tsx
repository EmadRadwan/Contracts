// REFACTOR: Fixed display of English class/type IDs in Arabic UI by avoiding fallback to raw English codes.
// Instead, use empty description or placeholder. Since ComboBox is virtual and bound by key (glAccountClassId),
// it will still be correctly selected — just shows ID until user interacts or we pre-fetch description.
// Best long-term: enrich query with Arabic descriptions.
glAccountTypeId: account?.glAccountTypeId
    ? {
        glAccountTypeId: account.glAccountTypeId,
        descriptionArabic: account.glAccountTypeDescription || "" // Don't show English ID
    }
    : null,

    glAccountClassId: account?.glAccountClassId
    ? {
        glAccountClassId: account.glAccountClassId,
        descriptionArabic: account.glAccountClassDescription || "" // Critical fix: was falling back to "SGA_EXPENSE"
    }
    : null,