import {createEntityAdapter, createSlice, EntityState, PayloadAction} from "@reduxjs/toolkit";
import {RootState} from "../../../app/store/configureStore";
import {OrderTerm} from "../../../app/models/order/orderTerm";

const orderTermsAdapter = createEntityAdapter<OrderTerm>({
    selectId: (orderTerm) => orderTerm.orderId!.concat(orderTerm.termTypeId),
});

interface OrderTermsState {
    orderTerms: EntityState<OrderTerm>;
    selectedOrderTerm: OrderTerm | undefined;
    relatedRecords: OrderTerm[] | undefined
}

const initialState: OrderTermsState = {
    orderTerms: orderTermsAdapter.getInitialState(),
    selectedOrderTerm: undefined,
    relatedRecords: undefined
}

export const orderTermsUiSlice = createSlice({
    name: "orderTermsUi",
    initialState,
    reducers: {
        setUiOrderTerms: (state, {payload}: PayloadAction<OrderTerm>) => {
            orderTermsAdapter.upsertOne(state.orderTerms, payload)
        },
        setRelatedRecords: (state, action) => {
            state.relatedRecords = action.payload;
        },
        setSelectedOrderTerm(state, action: PayloadAction<OrderTerm | undefined>) {
            state.selectedOrderTerm = action.payload
        },
        setUiOrderTermsFromApi: (state, action: PayloadAction<OrderTerm[]>) => {
            orderTermsAdapter.setAll(state.orderTerms, action.payload);
        },
        resetUiOrderTerms: (state) => {
            orderTermsAdapter.removeAll(state.orderTerms);
            state.selectedOrderTerm = undefined;
            state.relatedRecords = undefined;
        },
    }
})

export const {
    setUiOrderTerms,
    resetUiOrderTerms,
    setRelatedRecords,
    setSelectedOrderTerm,
    setUiOrderTermsFromApi
} = orderTermsUiSlice.actions;

export const orderTermsSelectors = orderTermsAdapter.getSelectors(
    (state: RootState) => state.orderTermsUi.orderTerms
);

export const {selectAll: orderTermsEntities} = orderTermsSelectors;