import {createEntityAdapter, createSlice, EntityState, PayloadAction} from "@reduxjs/toolkit";
import {RootState} from "../../../app/store/configureStore";
import {OrderItem} from "../../../app/models/order/orderItem";

const orderItemsAdapter = createEntityAdapter<OrderItem>({
    selectId: (orderItem) => orderItem.orderId!.concat(orderItem.orderItemSeqId),
});

interface OrderItemsState {
    orderItems: EntityState<OrderItem>;
    selectedOrderItem: OrderItem | undefined;
    relatedRecords: OrderItem[] | undefined

}

export const orderItemsInitialState: OrderItemsState = {
    orderItems: orderItemsAdapter.getInitialState(),
    selectedOrderItem: undefined,
    relatedRecords: undefined
};

export const orderItemsSlice = createSlice({
    name: "orderItemsUi",
    initialState: orderItemsInitialState,
    reducers: {
        setUiOrderItems: (state, action: PayloadAction<OrderItem[]>) => {
            orderItemsAdapter.upsertMany(state.orderItems, action.payload);
        },
        setProcessedOrderItems: (state, action: PayloadAction<OrderItem[]>) => {
            // loop thru payload, if isProductDeleted, then remove from state, else upsert
            action.payload.forEach((item) => {
                item.isProductDeleted ? orderItemsAdapter.removeOne(state.orderItems, item) :
                    orderItemsAdapter.upsertOne(state.orderItems, item);
            })
        },
        setUiOrderItemsFromApi: (state, action: PayloadAction<OrderItem[]>) => {
            orderItemsAdapter.setAll(state.orderItems, action.payload);
        },
        resetUiOrderItems: (state) => {
            orderItemsAdapter.removeAll(state.orderItems);
            state.selectedOrderItem = undefined;
            state.relatedRecords = undefined;
        },
        deletePromoOrderItem: (state, action) => {
            if (action.payload.length > 0) {
                orderItemsAdapter.upsertMany(state.orderItems, action.payload);
            }
        },
        setRelatedRecords: (state, action) => {
            state.relatedRecords = action.payload;
        },
        setSelectedOrderItem(state, action: PayloadAction<OrderItem | undefined>) {
            state.selectedOrderItem = action.payload
        },
    },
});

export const {
    setUiOrderItems,
    resetUiOrderItems, setProcessedOrderItems,
    deletePromoOrderItem,
    setRelatedRecords,
    setSelectedOrderItem,
    setUiOrderItemsFromApi
} = orderItemsSlice.actions;

export const orderItemsSelectors = orderItemsAdapter.getSelectors(
    (state: RootState) => state.orderItemsUi.orderItems
);

export const {selectAll: orderItemsEntities} = orderItemsSelectors;

