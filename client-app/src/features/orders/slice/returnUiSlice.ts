import {createEntityAdapter, createSelector, createSlice, EntityState, PayloadAction} from "@reduxjs/toolkit";
import {RootState} from "../../../app/store/configureStore";
import {ReturnItem} from "../../../app/models/order/returnItem";
import {Return} from "../../../app/models/order/return";
import {ReturnAdjustment} from "../../../app/models/order/returnAdjustment";
import {ReturnItemAndAdjustment} from "../../../app/models/order/returnItemAndAdjustment";

interface ReturnUiState {
    selectedSupplierId: string | undefined;
    selectedCustomerId: string | undefined;
    selectedProductId: string | undefined;
    selectedOrderId: string | undefined;
    selectedReturn: Return | undefined;
    selectedReturnItem: ReturnItem | undefined;
    currentReturnHeaderType: string | undefined;
    returnHeadersUi: EntityState<Return>;
    returnItemsUi: EntityState<ReturnItem>;
    returnItemsAndAdjustmentsUi: EntityState<ReturnItemAndAdjustment>;
    returnAdjustmentsUi: EntityState<ReturnAdjustment>;


}


const returnItemsAdapter = createEntityAdapter<ReturnItem>({
    selectId: (ReturnItem) => ReturnItem.orderId!.concat(ReturnItem.returnItemSeqId)
});

const returnItemsAndAdjustmentsAdapter = createEntityAdapter<ReturnItemAndAdjustment>({
    selectId: (ReturnItemAndAdjustment) => ReturnItemAndAdjustment.returnItemAndAdjustmentId
});

const returnHeadersAdapter = createEntityAdapter<Return>({
    selectId: (ReturnHeader) => ReturnHeader.returnId
});

const returnAdjsutmentsAdapter = createEntityAdapter<ReturnAdjustment>({
    selectId: (ReturnAdjustment) => ReturnAdjustment.returnAdjustmentId
});


export const initialState: ReturnUiState = {
    selectedSupplierId: undefined,
    selectedCustomerId: undefined,
    selectedProductId: undefined,
    selectedReturnItem: undefined,
    selectedOrderId: undefined,
    selectedReturn: undefined,
    currentReturnHeaderType: undefined,
    returnHeadersUi: returnHeadersAdapter.getInitialState(),
    returnItemsUi: returnItemsAdapter.getInitialState(),
    returnItemsAndAdjustmentsUi: returnItemsAndAdjustmentsAdapter.getInitialState(),
    returnAdjustmentsUi: returnAdjsutmentsAdapter.getInitialState()

}

export const returnUiSlice = createSlice({
    name: 'returnUi',
    initialState: initialState,
    reducers: {
        resetUiReturn: (state, action) => {
            state.returnItemsUi = returnItemsAdapter.getInitialState();
            state.returnHeadersUi = returnHeadersAdapter.getInitialState();
            state.returnAdjustmentsUi = returnAdjsutmentsAdapter.getInitialState();
            state.returnItemsAndAdjustmentsUi = returnItemsAndAdjustmentsAdapter.getInitialState();
            state.selectedSupplierId = undefined;
            state.selectedCustomerId = undefined;
            state.selectedReturnItem = undefined;
            state.selectedProductId = undefined;
        },
        setUiReturnItems: (state, action) => {
            if (action.payload.length === 0) {
                state.returnItemsUi = returnItemsAdapter.getInitialState();
            } else {
                returnItemsAdapter.upsertMany(state.returnItemsUi, action.payload);
            }
        },
        setUiReturnItemsAndAdjustments: (state, action) => {
            if (action.payload.length === 0) {
                state.returnItemsAndAdjustmentsUi = returnItemsAndAdjustmentsAdapter.getInitialState();
            } else {
                returnItemsAndAdjustmentsAdapter.upsertMany(state.returnItemsAndAdjustmentsUi, action.payload);
            }
        },
        setUiReturnAdjustments: (state, action) => {
            if (action.payload.length === 0) {
                state.returnAdjustmentsUi = returnAdjsutmentsAdapter.getInitialState();
            } else {
                returnAdjsutmentsAdapter.upsertMany(state.returnAdjustmentsUi, action.payload);
            }
        },
        setUiReturnHeaders: (state, action) => {
            // if payload is null then clear the state
            if (action.payload.length === 0) {
                state.returnHeadersUi = returnHeadersAdapter.getInitialState();
            } else {
                try {
                    returnHeadersAdapter.upsertMany(state.returnHeadersUi, action.payload);
                } catch (error) {
                    console.log('error from slice', error)
                }
            }
        },
        setUiNewReturnItems: (state, action) => {
            if (action.payload.length === 0) {
                state.returnItemsUi = returnItemsAdapter.getInitialState();
            } else {
                // if orderAdjustmentsUi is not empty then calculate the adjustments
                if (state.returnHeadersUi.ids.length > 0) {
                    returnItemsAdapter.removeAll(state.returnItemsUi);
                    returnItemsAdapter.setAll(state.returnItemsUi, action.payload);
                }
            }
        },


        setUiNewReturnHeaders: (state, action) => {
            // if payload is null then clear the state
            if (action.payload.length === 0) {
                state.returnHeadersUi = returnHeadersAdapter.getInitialState();
            } else {
                returnHeadersAdapter.removeAll(state.returnHeadersUi);
                returnHeadersAdapter.setAll(state.returnHeadersUi, action.payload);
            }

        },
        setSupplierId(state, action: PayloadAction<string | undefined>) {
            console.log('From Redux - SupplierId', action.payload)
            state.selectedSupplierId = action.payload
        },
        setSelectedReturn(state, {payload}) {
            state.selectedReturn = payload
        },
        setProductId(state, action: PayloadAction<string | undefined>) {
            state.selectedProductId = action.payload
        },
        setOrderId(state, action: PayloadAction<string | undefined>) {
            state.selectedOrderId = action.payload
        },
        setCustomerId(state, action: PayloadAction<string | undefined>) {
            state.selectedCustomerId = action.payload
            // clear the returnItemsUi and orderAdjustmentsUi if the customer is changed
            state.returnItemsUi = returnItemsAdapter.getInitialState();
            //state.orderAdjustmentsUi = orderAdjustmentsAdapter.getInitialState();
        },
        setSelectedReturnItem(state, action: PayloadAction<ReturnItem | undefined>) {
            state.selectedReturnItem = action.payload
        },
        setCurrentReturnHeaderType(state, action: PayloadAction<string | undefined>) {
            state.currentReturnHeaderType = action.payload
        },
    },
})

export const {
    setUiReturnItems,
    setUiReturnHeaders,
    setUiReturnAdjustments,
    setSupplierId,
    setOrderId,
    setSelectedReturnItem,
    setCustomerId,
    setProductId,
    setUiNewReturnHeaders,
    setUiNewReturnItems,
    resetUiReturn,
    setCurrentReturnHeaderType,
    setUiReturnItemsAndAdjustments,
    setSelectedReturn
} = returnUiSlice.actions;
export const returnHeadersSelectors = returnHeadersAdapter.getSelectors((state: RootState) => state.returnUi.returnHeadersUi);
export const returnItemsSelectors = returnItemsAdapter.getSelectors((state: RootState) => state.returnUi.returnItemsUi);
export const returnItemAndAdjustmentSelectors = returnItemsAndAdjustmentsAdapter.getSelectors((state: RootState) => state.returnUi.returnItemsAndAdjustmentsUi);
export const {selectAll: returnHeadersEntities} = returnHeadersSelectors
export const {selectAll: returnItemsEntities} = returnItemsSelectors
export const {selectAll: returnItemAndAdjustmentssEntities} = returnItemAndAdjustmentSelectors

const returnUiSelector = (state: RootState) => state.returnUi

export const returnItemsAndAdjustmentsSelector = createSelector(
    returnUiSelector,
    (returnUiSelector) => {
        return Object.values(returnUiSelector.returnItemsAndAdjustmentsUi.entities)
    })











