import {createEntityAdapter, createSelector, createSlice, EntityState, PayloadAction} from "@reduxjs/toolkit";
import {RootState} from "../../../app/store/configureStore";
import {Payment} from "../../../app/models/accounting/payment";

const orderPaymentsAdapter = createEntityAdapter<Payment>({
    selectId: (payment) => payment.paymentId,
});

interface BillingAccount {
    billingAccountId: string,
    description: string
}

interface OrderPaymentsState {
    orderPayments: EntityState<Payment>;
    customerBillingAccount: BillingAccount | undefined
    billingAccountPayment: { billingAccountId: BillingAccount, useUpToFromBillingAccount: number } | undefined

}

export const orderPaymentsInitialState: OrderPaymentsState = {
    orderPayments: orderPaymentsAdapter.getInitialState(),
    customerBillingAccount: undefined,
    billingAccountPayment: undefined,
};
export const orderPaymentsSlice = createSlice({
    name: "orderPaymentsUi",
    initialState: orderPaymentsInitialState,
    reducers: {
        setUiOrderPayments: (state, action: PayloadAction<Payment[]>) => {
            orderPaymentsAdapter.setAll(state.orderPayments, action.payload);
        },
        resetUiOrderPayments: (state) => {
            orderPaymentsAdapter.removeAll(state.orderPayments);
            state.customerBillingAccount = undefined;
            state.billingAccountPayment = undefined;
        },
        deletePayment: (state, action) => {
            orderPaymentsAdapter.removeMany(
                state.orderPayments, action.payload)
        },
        updatePayment: (state, action) => {
            orderPaymentsAdapter.upsertOne(state.orderPayments, action.payload)
        },
        setCustomerBillingAccount(state, action: PayloadAction<any>) {
            if (action.payload && action.payload.billingAccountId !== "") {
                state.customerBillingAccount = action.payload
            } else {
                state.customerBillingAccount = undefined
            }
        },
        setBillingAccountPayment(state, action: PayloadAction<{ billingAccountId: BillingAccount, useUpToFromBillingAccount: number } | undefined>) {

            if (action.payload?.billingAccountId) {
                if (action.payload?.billingAccountId?.billingAccountId !== "" && action.payload?.useUpToFromBillingAccount > 0) {
                    state.billingAccountPayment = action.payload
                }
            } else {
                state.billingAccountPayment = undefined
            }
        }
    },
});

export const {
    setUiOrderPayments,
    resetUiOrderPayments,
    deletePayment,
    setCustomerBillingAccount,
    setBillingAccountPayment,
    updatePayment
} = orderPaymentsSlice.actions;
export const orderPaymentsSelectors = orderPaymentsAdapter.getSelectors(
    (state: RootState) => state.orderPaymentsUi.orderPayments
);

export const {selectAll: orderPaymentsEntities} = orderPaymentsSelectors;