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

export const accountingSharedInitialState: AccountingSharedState = {
    acctgTransEntries: acctgTransEntryAdapter.getInitialState(),
    selectedFixedAsset: undefined,
    selectedAccountingCompanyId: undefined,
    selectedAccountingCompanyName: undefined,
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
        },
        setSelectedAccountingCompanyName(state, action: PayloadAction<string>) {
            state.selectedAccountingCompanyName = action.payload;
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
});

export const {
    setSelectedAccountingCompanyId, setSelectedAccountingCompanyName, setWhatWasClicked, setSelectedBillingAccount, setSelectedFinancialAccount,
    setUiAcctgTransEntriesFromApi, setSelectedPaymentGroup, setPaymentGroupMemberFormEditMode, setSelectedPaymentGroupMember, setSelectedFixedAsset, setSelectedInvoice, setSeletedCustomTimePeriodId, setSelectedAgreement, setSelectedPayment, setSelectedOrder
} = accountingSharedSlice.actions;

export const acctgTransEntriesSelectors = acctgTransEntryAdapter.getSelectors(
    (state: RootState) => state.accountingSharedUi.acctgTransEntries
);

export const {selectAll: acctgTransEntriesEntities} = acctgTransEntriesSelectors;

