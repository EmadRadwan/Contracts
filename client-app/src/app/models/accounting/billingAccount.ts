export interface BillingAccount {
    billingAccountId: string;
    accountLimit?: number | null;
    accountCurrencyUomId?: string;
    accountCurrencyUomDescription?: string;
    partyId?: any;
    partyName?: string;
    fromDate?: Date | null;
    thruDate?: Date | null;
    description?: string;
    availableBalance?: number
}