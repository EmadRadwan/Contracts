import {createEntityAdapter, createSlice, EntityState, PayloadAction} from "@reduxjs/toolkit";
import {RootState} from "../../../app/store/configureStore";
import {OrderAdjustment} from "../../../app/models/order/orderAdjustment";
import {setNeedsTaxRecalculation} from "./sharedOrderUiSlice";

const orderAdjustmentsAdapter = createEntityAdapter<OrderAdjustment>({
    selectId: (orderAdjustment) => orderAdjustment.orderAdjustmentId,
});

interface OrderAdjustmentsState {
    orderAdjustments: EntityState<OrderAdjustment>;
}

export const orderAdjustmentsInitialState: OrderAdjustmentsState = {
    orderAdjustments: orderAdjustmentsAdapter.getInitialState(),
};

// Special Remark: Counter for unique fallback keys
let fallbackKeyCounter = 0;

export const orderAdjustmentsSlice = createSlice({
    name: "orderAdjustmentsUi",
    initialState: orderAdjustmentsInitialState,
    reducers: {
        setUiOrderAdjustments: (state, action: PayloadAction<OrderAdjustment[]>) => {
            const validatedAdjustments = action.payload.filter((adj) => {
                if (!adj.orderAdjustmentTypeId || adj.amount === undefined) {
                    console.warn("DEBUG: Invalid adjustment skipped:", JSON.stringify(adj, null, 2));
                    return false;
                }
                return true;
            });

            const newEntities = validatedAdjustments.reduce((acc, adj) => {
                const key = adj.orderAdjustmentId || `adj-${adj.orderItemSeqId || "order"}-${fallbackKeyCounter++}`;
                acc[key] = adj;
                return acc;
            }, {} as { [key: string]: OrderAdjustment });

            state.orderAdjustments.entities = newEntities;
            state.orderAdjustments.ids = Object.keys(newEntities);
        },
        setUiTaxAdjustments: (state, action: PayloadAction<OrderAdjustment[]>) => {
            const validatedAdjustments = action.payload.filter((adj) => {
                if (!adj.orderAdjustmentTypeId || adj.amount === undefined) {
                    console.warn("DEBUG: Invalid adjustment skipped:", JSON.stringify(adj, null, 2));
                    return false;
                }
                return true;
            });

            // Preserve non-tax adjustments
            const nonTaxAdjustments = Object.fromEntries(
                Object.entries(state.orderAdjustments.entities).filter(
                    ([_, adj]) => adj.orderAdjustmentTypeId !== 'VAT_TAX'
                )
            );

            const newEntities = validatedAdjustments.reduce((acc, adj) => {
                const key = adj.orderAdjustmentId || `adj-${adj.orderItemSeqId || "order"}-${fallbackKeyCounter++}`;
                acc[key] = adj;
                return acc;
            }, { ...nonTaxAdjustments } as { [key: string]: OrderAdjustment });

            state.orderAdjustments.entities = newEntities;
            state.orderAdjustments.ids = Object.keys(newEntities);
        },
        resetUiOrderAdjustments: (state) => {
            state.orderAdjustments.entities = {};
            state.orderAdjustments.ids = [];
            // Special Remark: Reset counter for consistency
            fallbackKeyCounter = 0;
        },
        setUiOrderAdjustmentsFromApi: (state, action: PayloadAction<OrderAdjustment[]>) => {
            orderAdjustmentsAdapter.setAll(state.orderAdjustments, action.payload);
        },
        deletePromoOrderAdjustments: (state, action) => {
            if (action.payload.length > 0) {
                orderAdjustmentsAdapter.upsertMany(
                    state.orderAdjustments,
                    action.payload,
                );
            }
        },
        deleteTaxAdjustments: (state, action) => {
            if (action.payload.length > 0) {
                orderAdjustmentsAdapter.upsertMany(
                    state.orderAdjustments,
                    action.payload,
                );
            }
        },
    },
});

/*// Thunk to update adjustments and trigger tax recalculation if needed
export const updateOrderAdjustments = (
    adjustments: OrderAdjustment[]
): AppThunk => (dispatch, getState) => {
    dispatch(setUiOrderAdjustments(adjustments));
    const state = getState() as RootState;
    const addTax = state.sharedOrderUi.addTax;
    if (addTax) {
        dispatch(setNeedsTaxRecalculation(true));
    }
};

// Thunk to update tax adjustments without triggering recalculation
export const updateTaxAdjustments = (
    adjustments: OrderAdjustment[]
): AppThunk => (dispatch) => {
    dispatch(setUiTaxAdjustments(adjustments));
};

// Thunk to delete promo adjustments and trigger tax recalculation if needed
export const removePromoAdjustments = (
    adjustments: OrderAdjustment[]
): AppThunk => (dispatch, getState) => {
    dispatch(deletePromoOrderAdjustments(adjustments));
    const state = getState() as RootState;
    const addTax = state.sharedOrderUi.addTax;
    if (addTax) {
        dispatch(setNeedsTaxRecalculation(true));
    }
};

// Thunk to delete tax adjustments
export const removeTaxAdjustments = (
    adjustments: OrderAdjustment[]
): AppThunk => (dispatch) => {
    dispatch(deleteTaxAdjustments(adjustments));
};*/

export const {
    setUiOrderAdjustments,
    setUiTaxAdjustments,
    resetUiOrderAdjustments,
    deletePromoOrderAdjustments,
    deleteTaxAdjustments,
    setUiOrderAdjustmentsFromApi
} = orderAdjustmentsSlice.actions;

export const orderAdjustmentsSelectors = orderAdjustmentsAdapter.getSelectors(
    (state: RootState) => state.orderAdjustmentsUi.orderAdjustments
);

export const {selectAll: orderAdjustmentsEntities} = orderAdjustmentsSelectors;
