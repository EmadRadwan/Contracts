export interface AcctgTransEntry {
  /** set on a reversing entry: id of the transaction it cancels */
  reversalOfAcctgTransId?: string | null;
  /** set on a reversed original: id of the reversal that cancelled it */
  reversedByAcctgTransId?: string | null;
    acctgTransId: string;
    acctgTransEntrySeqId: string;
    acctgTransEntryTypeId?: string | null;
    acctgTransEntryTypeDescription?: string | null;
    description?: string | null;
    voucherRef?: string | null;
    partyId?: string | null;
    roleTypeId?: string | null;
    theirPartyId?: string | null;
    productId?: string | null;
    productName?: string | null;
    theirProductId?: string | null;
    inventoryItemId?: string | null;
    glAccountTypeId?: string | null;
    glAccountId?: string | null;
    organizationPartyId?: string | null;
    amount?: number | null;
    currencyUomId?: string | null;
    origAmount?: number | null;
    origCurrencyUomId?: string | null;
    debitCreditFlag?: string | null;
    dueDate?: Date | null;
    groupId?: string | null;
    taxId?: string | null;
    reconcileStatusId?: string | null;
    settlementTermId?: string | null;
    isSummary?: string | null;
    isAcctgTransEntryDeleted?: boolean | null;
}
