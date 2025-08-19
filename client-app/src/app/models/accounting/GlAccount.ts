export interface GlAccount {
    glAccountId?: string | null;
    glAccountTypeId?: string | null;
    glAccountClassId?: string | null;
    glResourceTypeId?: string | null;
    parentGlAccountId?: string | null;
    accountCode?: string | null;
    accountName?: string | null;
    level?: number | null;

    items?: GlAccount[];
}