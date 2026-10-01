import {createEntityAdapter, createSlice, EntityState, PayloadAction} from "@reduxjs/toolkit";
import {AcctgTransEntry} from "../../../app/models/accounting/acctgTransEntry";
import {RootState} from "../../../app/store/configureStore";
import { FixedAsset } from "../../../app/models/accounting/fixedAsset";
import { Invoice } from "../../../app/models/accounting/invoice";
import { Agreement } from "../../../app/models/accounting/agreement";
import { BillingAccount } from "../../../app/models/accounting/billingAccount";
import { Payment } from "../../../app/models/accounting/payment";
import { Order } from "../../../app/models/order/order";
import { FinancialAccount } from "../../../app/models/accounting/financialAccount";
import { PaymentGroup } from "../../../app/models/accounting/paymentGroup";

const acctgTransEntryAdapter = createEntityAdapter<AcctgTransEntry>({
    selectId: (acctgTransEntry) => acctgTransEntry.acctgTransId!.concat(acctgTransEntry.acctgTransEntrySeqId),
});

interface AccountingSharedState {
    acctgTransEntries: EntityState<AcctgTransEntry>;
    selectedFixedAsset: FixedAsset | undefined
    selectedAccountingCompanyId: string | undefined;
    selectedAccountingCompanyName: string | undefined;
    whatWasClicked: string;
    selectedInvoice: Invoice | undefined
    seletedCustomTimePeriodId: string | undefined
    selectedAgreement: Agreement | undefined
    selectedBillingAccount: BillingAccount | undefined
    selectedFinancialAccount: FinancialAccount | undefined
    selectedPayment: Payment | undefined
    selectedOrder: Order | undefined
    selectedPaymentGroup: PaymentGroup | undefined
    selectedPaymentGroupMember: any
    paymentGroupMemberFormEditMode: number
}

// The accounting company is picked once from the companies list (OrganizationGlSettingsList) and
// every accounting screen reads it from here. It used to live in memory only, so a page reload
// wiped it and all ten company-scoped reports bounced back to /orgGl — the two trial balances
// only escaped that by reading the logged-in user's own org instead, which is why they behave
// differently from the rest. Persisting the choice fixes every consumer at the source.
//
// Wrapped because storage access throws in private mode / when site data is blocked, and the app
// must still start with nothing selected in that case.
const COMPANY_ID_KEY = "selectedAccountingCompanyId";
const COMPANY_NAME_KEY = "selectedAccountingCompanyName";

const readStored = (key: string): string | undefined => {
    try {
        return localStorage.getItem(key) ?? undefined;
    } catch {
        return undefined;
    }
};

const writeStored = (key: string, value: string) => {
    try {
        localStorage.setItem(key, value);
    } catch {
        /* storage unavailable — the selection simply stays in memory for this session */
    }
};

const clearStored = (key: string) => {
    try {
        localStorage.removeItem(key);
    } catch {
        /* nothing to do — see writeStored */
    }
};

export const accountingSharedInitialState: AccountingSharedState = {
    acctgTransEntries: acctgTransEntryAdapter.getInitialState(),
    selectedFixedAsset: undefined,
    selectedAccountingCompanyId: readStored(COMPANY_ID_KEY),
    selectedAccountingCompanyName: readStored(COMPANY_NAME_KEY),
    whatWasClicked: "",
    selectedInvoice: undefined,
    seletedCustomTimePeriodId: undefined,
    selectedAgreement: undefined,
    selectedBillingAccount: undefined,
    selectedPayment : undefined,
    selectedOrder: undefined,
    selectedFinancialAccount: undefined,
    selectedPaymentGroup: undefined,
    selectedPaymentGroupMember: undefined,
    paymentGroupMemberFormEditMode: 0
};


export const accountingSharedSlice = createSlice({
    name: "accountingSharedUi",
    initialState: accountingSharedInitialState,
    reducers: {
        setSelectedAccountingCompanyId(state, action: PayloadAction<string>) {
            state.selectedAccountingCompanyId = action.payload;
            writeStored(COMPANY_ID_KEY, action.payload);
        },
        setSelectedAccountingCompanyName(state, action: PayloadAction<string>) {
            state.selectedAccountingCompanyName = action.payload;
            writeStored(COMPANY_NAME_KEY, action.payload);
        },
        setWhatWasClicked(state, action: PayloadAction<string>) {
            state.whatWasClicked = action.payload;
        },
        setUiAcctgTransEntriesFromApi: (state, action: PayloadAction<AcctgTransEntry[]>) => {
            acctgTransEntryAdapter.setAll(state.acctgTransEntries, action.payload);
        },
        setSelectedFixedAsset(state, action: PayloadAction<FixedAsset | undefined>) {
            state.selectedFixedAsset = action.payload
        },
        setSelectedInvoice(state, action: PayloadAction<Invoice | undefined>) {
            state.selectedInvoice = action!.payload
        },
        setSeletedCustomTimePeriodId(state, action: PayloadAction<string | undefined>) {
            state.seletedCustomTimePeriodId = action.payload
        },
        setSelectedAgreement(state, {payload}: {payload: Agreement | undefined}) {
            state.selectedAgreement = payload
        },
        setSelectedBillingAccount(state, {payload}: {payload: BillingAccount | undefined}) {
            state.selectedBillingAccount = payload
        },
        setSelectedPayment(state, {payload}: {payload: Payment | undefined}) {
            state.selectedPayment = payload
        },
        setSelectedOrder(state, {payload}: {payload: Order | undefined}) {
            state.selectedOrder = payload
        },
        setSelectedFinancialAccount(state, {payload}: {payload: FinancialAccount | undefined}) {
            state.selectedFinancialAccount = payload
        },
        setSelectedPaymentGroup(state, {payload}: {payload: PaymentGroup | undefined}) {
            state.selectedPaymentGroup = payload
        },
        setSelectedPaymentGroupMember(state, {payload}: {payload: any}) {
            state.selectedPaymentGroupMember = payload
        },
        setPaymentGroupMemberFormEditMode(state, {payload}: {payload: number}) {
            state.paymentGroupMemberFormEditMode = payload
        }
    },
    extraReducers: (builder) => {
        // signOut only clears the user; the rest of the store is left standing. Now that the
        // company selection is persisted it would otherwise outlive the session and the next
        // user to sign in on this browser would silently inherit the previous one's company.
        // Session expiry (fetchCurrentUser.rejected) deliberately does not clear it — that is
        // the same user coming back, and re-picking the company would just be friction.
        //
        // Matched by action type rather than by importing accountSlice's `signOut`: accountSlice
        // imports the router, the router imports the accounting screens, and those import this
        // slice — so an import here closes a cycle that would leave `signOut` undefined at the
        // moment createSlice runs. Keep it a string.
        builder.addCase("account/signOut", (state) => {
            state.selectedAccountingCompanyId = undefined;
            state.selectedAccountingCompanyName = undefined;
            clearStored(COMPANY_ID_KEY);
            clearStored(COMPANY_NAME_KEY);
        });
    },
});

export const {
    setSelectedAccountingCompanyId, setSelectedAccountingCompanyName, setWhatWasClicked, setSelectedBillingAccount, setSelectedFinancialAccount,
    setUiAcctgTransEntriesFromApi, setSelectedPaymentGroup, setPaymentGroupMemberFormEditMode, setSelectedPaymentGroupMember, setSelectedFixedAsset, setSelectedInvoice, setSeletedCustomTimePeriodId, setSelectedAgreement, setSelectedPayment, setSelectedOrder
} = accountingSharedSlice.actions;

export const acctgTransEntriesSelectors = acctgTransEntryAdapter.getSelectors(
    (state: RootState) => state.accountingSharedUi.acctgTransEntries
);

export const {selectAll: acctgTransEntriesEntities} = acctgTransEntriesSelectors;

