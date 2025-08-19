import {AcctgTransEntry} from "./acctgTransEntry";

export interface CreateAcctgTransAndEntriesParams {
    acctgTransId?: string;
    glFiscalTypeId?: string;
    acctgTransTypeId?: string;
    invoiceId?: string;
    paymentId?: string;
    partyId?: string;
    fromPartyId?: any;
    productId?: any;
    shipmentId?: string;
    roleTypeId?: string;
    description?: string;
    isPosted: string
    creditGlAccountId?: string
    debitGlAccountId?: string
    organizationPartyId?: string
    acctgTransEntries?: AcctgTransEntry[];
    transactionDate?: Date;
}