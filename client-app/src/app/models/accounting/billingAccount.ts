/*export interface BillingAccount {
    billingAccountId: string;
    accountLimit?: number | null;
    accountCurrencyUomId?: string;
    accountCurrencyUomDescription?: string;
    partyId?: any;
    projectId?: any;
    partyName?: string;
    fromDate?: Date | null;
    thruDate?: Date | null;
    description?: string;
    availableBalance?: number
}*/

// src/app/models/accounting/billingAccount.ts
export interface BillingAccount {
    billingAccountId?: string;
    partyId?: string | { fromPartyId: string; fromPartyName: string }; // allow both for flexibility
    partyName?: string;
    projectId?: string | { projectId: string; ProjectName: string };
    projectName?: string;
    accountLimit?: number;
    availableBalance?: number;
    fromDate?: Date | string;
    thruDate?: Date | string | null;
    description?: string;
    createdDate?: string;
}