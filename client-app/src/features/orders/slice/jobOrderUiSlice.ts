import {createEntityAdapter, createSelector, createSlice, EntityState, PayloadAction,} from "@reduxjs/toolkit";
import {RootState} from "../../../app/store/configureStore";
import {Payment} from "../../../app/models/accounting/payment";
import {OrderItem} from "../../../app/models/order/orderItem";
import {OrderAdjustment} from "../../../app/models/order/orderAdjustment";
import {Vehicle} from "../../../app/models/service/vehicle";

interface JobOrderUiState {
    selectedCustomerId: string | undefined;
    selectedProductId: string | undefined;
    selectedOrderItem: OrderItem | undefined;
    selectedProduct: any | undefined;
    selectedVehicleId: string | undefined;
    selectedVehicle: Vehicle | undefined;
    jobOrderAdjustmentsUi: EntityState<OrderAdjustment>;
    jobOrderItemsUi: EntityState<OrderItem>;
    jobOrderPaymentsUi: EntityState<Payment>;
    relatedRecords: OrderItem[] | undefined;
    isSelectedServicePriceZero: boolean;
    isNewServiceRateAndSpecificationAdded: boolean;
}

function calculateOrderItemAdjustments(
    items: OrderItem[],
    adjustments: OrderAdjustment[],
): OrderItem[] {
    console.log(
        "%c calculateOrderItemAdjustments:",
        "color: red; font-weight: bold;",
    );

    const adjustedItems = items.map((orderItem: OrderItem) => {
        const orderItemManualAdjustments = calculateAdjustments(
            orderItem,
            true,
            adjustments,
        );
        const orderItemAutoAdjustments = calculateAdjustments(
            orderItem,
            false,
            adjustments,
        );

        const calculateProperties = (subtotalWithoutAdjustments: number) => {
            return {
                // Adjusted properties
                totalItemAdjustmentsManual: orderItemManualAdjustments,
                totalItemAdjustmentsAuto: orderItemAutoAdjustments,
                totalItemAdjustments:
                    orderItemManualAdjustments + orderItemAutoAdjustments,
                subTotal:
                    subtotalWithoutAdjustments +
                    orderItemManualAdjustments +
                    orderItemAutoAdjustments,
                orderUnitPrice:
                    (subtotalWithoutAdjustments +
                        orderItemManualAdjustments +
                        orderItemAutoAdjustments) /
                    orderItem.quantity,
            };
        };

        const subTotalWithoutAdjustments = orderItem.unitListPrice
            ? orderItem.unitListPrice * orderItem.quantity
            : orderItem.unitPrice * orderItem.quantity;

        return {
            ...orderItem,
            ...calculateProperties(subTotalWithoutAdjustments),
        };
    });

    return adjustedItems;
}

function calculateAdjustments(
    orderItem: OrderItem,
    isManual: boolean,
    adjustments: OrderAdjustment[],
): number {
    console.log(
        "%c calculatedAdjustments:",
        "color: blue; font-weight: bold;",
        orderItem,
        adjustments,
    );

    // Filter adjustments based on the provided criteria
    const filteredAdjustments = adjustments.filter(
        (adjustment: OrderAdjustment) => {
            return (
                adjustment.orderItemSeqId !== "_NA_" &&
                !adjustment.isAdjustmentDeleted &&
                adjustment.isManual === (isManual ? "Y" : "N") &&
                adjustment.orderItemSeqId === orderItem.orderItemSeqId
            );
        },
    );

    console.log(
        "%c filteredAdjustments:",
        "color: blue; font-weight: bold;",
        filteredAdjustments,
    );

    // Calculate adjustments based on the filtered adjustments
    const adjustmentsTotal = filteredAdjustments.reduce(
        (sum: number, record: OrderAdjustment) => {
            const amount = record.amount || 0;
            const quantity = orderItem.quantity || 1;
            return sum + (isManual ? amount * quantity : amount);
        },
        0,
    );

    return adjustmentsTotal;
}

const orderItemsAdapter = createEntityAdapter<OrderItem>({
    selectId: (OrderItem) => OrderItem.orderId!.concat(OrderItem.orderItemSeqId),
});

const orderAdjustmentsAdapter = createEntityAdapter<OrderAdjustment>({
    selectId: (OrderAdjustment) => OrderAdjustment.orderAdjustmentId,
});

const orderPaymentsAdapter = createEntityAdapter<Payment>({
    selectId: (Payment) => Payment.paymentId,
});

export const initialState: JobOrderUiState = {
    selectedCustomerId: undefined,
    selectedProductId: undefined,
    selectedOrderItem: undefined,
    selectedProduct: undefined,
    selectedVehicleId: undefined,
    selectedVehicle: undefined,
    jobOrderAdjustmentsUi: orderAdjustmentsAdapter.getInitialState(),
    jobOrderItemsUi: orderItemsAdapter.getInitialState(),
    jobOrderPaymentsUi: orderPaymentsAdapter.getInitialState(),
    relatedRecords: undefined,
    isSelectedServicePriceZero: false,
    isNewServiceRateAndSpecificationAdded: false,
};

export const jobOrderUiSlice = createSlice({
    name: "jobOrderUi",
    initialState: initialState,
    reducers: {
        resetUiJobOrder: (state, action) => {
            state.jobOrderItemsUi = orderItemsAdapter.getInitialState();
            state.jobOrderAdjustmentsUi = orderAdjustmentsAdapter.getInitialState();
            state.jobOrderPaymentsUi = orderPaymentsAdapter.getInitialState();
            state.selectedCustomerId = undefined;
            state.selectedOrderItem = undefined;
            state.selectedProductId = undefined;
            state.selectedProduct = undefined;
            state.selectedVehicleId = undefined;
        },
        setUiJobOrderItems: (state, action) => {
            if (action.payload.length === 0) {
                state.jobOrderItemsUi = orderItemsAdapter.getInitialState();
            } else {
                orderItemsAdapter.upsertMany(state.jobOrderItemsUi, action.payload);
            }
        },
        setUiJobOrderPayments: (state, action) => {
            if (action.payload.length === 0) {
                state.jobOrderPaymentsUi = orderPaymentsAdapter.getInitialState();
            } else {
                orderPaymentsAdapter.upsertMany(
                    state.jobOrderPaymentsUi,
                    action.payload,
                );
            }
        },
        setUiNewJobOrderPayments: (state, action) => {
            if (action.payload.length === 0) {
                state.jobOrderPaymentsUi = orderPaymentsAdapter.getInitialState();
            } else {
                orderPaymentsAdapter.removeAll(state.jobOrderPaymentsUi);
                orderPaymentsAdapter.setAll(state.jobOrderPaymentsUi, action.payload);
            }
        },
        deletePayment: (state, action) => {
            try {
                if (action.payload.length !== 0) {
                    orderPaymentsAdapter.removeMany(
                        state.jobOrderPaymentsUi,
                        action.payload,
                    );
                }
            } catch (error) {
                console.log("error deleting payment", error);
            }
        },

        deletePromoOrderItem: (state, action) => {
            if (action.payload.length > 0) {
                orderItemsAdapter.removeOne(state.jobOrderItemsUi, action.payload);
            }
        },
        deletePromoOrderAdjustments: (state, action) => {
            if (action.payload.length > 0) {
                // set isAdjustmentDeleted to true for the adjustments in the payload
                action.payload.forEach((adjustmentId: string) => {
                    state.jobOrderAdjustmentsUi!.entities[
                        adjustmentId
                        ]!.isAdjustmentDeleted = true;
                });
            }
        },
        setUiJobOrderAdjustments: (state, action) => {
            // if payload is null then clear the state
            if (action.payload.length === 0) {
                state.jobOrderAdjustmentsUi = orderAdjustmentsAdapter.getInitialState();
            } else {
                orderAdjustmentsAdapter.upsertMany(
                    state.jobOrderAdjustmentsUi,
                    action.payload,
                );
            }
        },
        setUiNewJobOrderItems: (state, action) => {
            if (action.payload.length === 0) {
                state.jobOrderItemsUi = orderItemsAdapter.getInitialState();
            } else {
                orderItemsAdapter.setAll(state.jobOrderItemsUi, action.payload);
            }
        },
        setUiNewJobOrderAdjustments: (state, action) => {
            if (action.payload.length === 0) {
                state.jobOrderAdjustmentsUi = orderAdjustmentsAdapter.getInitialState();
            } else {
                orderAdjustmentsAdapter.setAll(
                    state.jobOrderAdjustmentsUi,
                    action.payload,
                );
            }
        },
        setProductId(state, action: PayloadAction<string | undefined>) {
            state.selectedProductId = action.payload;
        },
        setSelectedProduct(state, action: PayloadAction<any | undefined>) {
            state.selectedProduct = action.payload;
        },
        setVehicleId(state, action: PayloadAction<string | undefined>) {
            state.selectedVehicleId = action.payload;
        },
        setSelectedVehicle(state, action: PayloadAction<Vehicle | undefined>) {
            state.selectedVehicle = action.payload;
        },
        setCustomerId(state, action: PayloadAction<string | undefined>) {
            state.selectedCustomerId = action.payload;
            state.jobOrderItemsUi = orderItemsAdapter.getInitialState();
            state.jobOrderAdjustmentsUi = orderAdjustmentsAdapter.getInitialState();
        },
        setSelectedOrderItem(state, action: PayloadAction<OrderItem | undefined>) {
            state.selectedOrderItem = action.payload;
        },
        setRelatedRecords(state, action: PayloadAction<OrderItem[] | undefined>) {
            state.relatedRecords = action.payload;
        },
        setIsSelectedPriceZero(state, action: PayloadAction<boolean>) {
            state.isSelectedServicePriceZero = action.payload;
        },
        setIsNewServiceRateAndSpecificationAdded(
            state,
            action: PayloadAction<boolean>,
        ) {
            state.isNewServiceRateAndSpecificationAdded = action.payload;
        },
    },
});

export const {
    setUiJobOrderItems,
    setUiJobOrderAdjustments,
    setSelectedOrderItem,
    setCustomerId,
    setProductId,
    setUiJobOrderPayments,
    setUiNewJobOrderAdjustments,
    setUiNewJobOrderItems,
    resetUiJobOrder,
    deletePromoOrderItem,
    deletePromoOrderAdjustments,
    deletePayment,
    setUiNewJobOrderPayments,
    setIsSelectedPriceZero,
    setIsNewServiceRateAndSpecificationAdded,
    setRelatedRecords,
    setVehicleId,
    setSelectedVehicle,
    setSelectedProduct,
} = jobOrderUiSlice.actions;
export const orderAdjustmentSelectors = orderAdjustmentsAdapter.getSelectors(
    (state: RootState) => state.jobOrderUi.jobOrderAdjustmentsUi,
);
export const orderItemsSelectors = orderItemsAdapter.getSelectors(
    (state: RootState) => state.jobOrderUi.jobOrderItemsUi,
);
export const orderPaymentsSelectors = orderPaymentsAdapter.getSelectors(
    (state: RootState) => state.jobOrderUi.jobOrderPaymentsUi,
);
export const {selectAll: orderAdjustmentsEntities} = orderAdjustmentSelectors;
export const {selectAll: orderItemsEntities} = orderItemsSelectors;
export const {selectAll: orderPaymentsEntities} = orderPaymentsSelectors;

const jobOrderUiSelector = (state: RootState) => state.jobOrderUi;
export const jobOrderItemsSelector = createSelector(
    jobOrderUiSelector,
    (jobOrderUi) => {

        return Object.values(jobOrderUi.jobOrderItemsUi.entities).filter(
            (orderItem) => {
                if (!orderItem!.isProductDeleted) return orderItem;
            },
        );
    },
);

export const orderPaymentsSelector = createSelector(
    jobOrderUiSelector,
    (jobOrderUi) => {

        return Object.values(jobOrderUi.jobOrderPaymentsUi.entities).filter(
            (payment) => {
                if (!payment!.isPaymentDeleted) return payment;
            },
        );
    },
);

export const jobOrderAdjustmentsSelector = createSelector(
    jobOrderUiSelector,
    (orderUi) => {
        return Object.values(orderUi.jobOrderAdjustmentsUi.entities).filter(
            (orderAdjustment) => {
                if (!orderAdjustment!.isAdjustmentDeleted) return orderAdjustment;
            },
        );
    },
);

export const jobOrderLevelAdjustments = createSelector(
    jobOrderUiSelector,
    (orderUi) => {
        return Object.values(orderUi.jobOrderAdjustmentsUi.entities).filter(
            (orderAdjustment) => {
                if (
                    orderAdjustment!.orderItemSeqId === "_NA_" &&
                    !orderAdjustment!.isAdjustmentDeleted
                )
                    return orderAdjustment;
            },
        );
    },
);

// create a selector to get the orderItemAdjustments for the selected order
// using the orderAdjustmentsUi and the selectedOrderId
// and the CreateSelector from reselect
export const jobOrderItemAdjustmentsSelector = createSelector(
    (state: { jobOrderUi: { jobOrderAdjustmentsUi: any } }) =>
        state.jobOrderUi.jobOrderAdjustmentsUi,
    (state: { jobOrderUi: { selectedOrderItem: any } }) =>
        state.jobOrderUi.selectedOrderItem,
    (state, orderItem) => {
        console.log("%c state:", "color: red; font-weight: bold;", state);
        console.log(
            "%c selectedOrderItem:",
            "color: blue; font-weight: bold;",
            orderItem,
        );

        return (
            orderItem &&
            Object.values(state.entities).filter((orderAdjustment: any) => {
                if (
                    orderAdjustment!.orderId === orderItem.orderId &&
                    !orderAdjustment!.isAdjustmentDeleted &&
                    orderAdjustment.orderItemSeqId === orderItem.orderItemSeqId
                )
                    return orderAdjustment;
            })
        );
    },
);

export const selectAdjustedOrderItems = createSelector(
    [orderItemsEntities, orderAdjustmentsEntities],
    (orderItems, orderAdjustments) => {
        const calcResult = calculateOrderItemAdjustments(
            Object.values(orderItems) as OrderItem[],
            Object.values(orderAdjustments) as OrderAdjustment[],
        );
        return calcResult.filter((orderItem) => {
            if (!orderItem!.isProductDeleted) return orderItem;
        });
    },
);

export const selectAdjustedOrderItemsWithMarkedForDeletionItems =
    createSelector(
        [orderItemsEntities, orderAdjustmentsEntities],
        (orderItems, orderAdjustments) => {
            return calculateOrderItemAdjustments(
                Object.values(orderItems) as OrderItem[],
                Object.values(orderAdjustments) as OrderAdjustment[],
            );
        },
    );

export const jobOrderSubTotal = createSelector(
    selectAdjustedOrderItems,
    (adjustedOrderItems) => {
        if (!adjustedOrderItems) {
            return 0;
        }

        return adjustedOrderItems.reduce((sum, record) => {
            return sum + (record.subTotal ?? 0); // Use optional chaining and nullish coalescing
        }, 0);
    },
);

export const jobOrderItemSubTotal = createSelector(
    jobOrderUiSelector,
    (state: { orderUi: { selectedOrderItem: any } }) =>
        state.orderUi.selectedOrderItem,
    (orderUi) => {
        const filteredOrderItems = Object.values(
            orderUi.jobOrderItemsUi.entities,
        ).filter(
            (item) =>
                !item!.isProductDeleted &&
                item!.orderItemSeqId === orderUi.selectedOrderItem?.orderItemSeqId,
        );

        return (
            filteredOrderItems &&
            filteredOrderItems!.reduce((sum: any, record: any) => {
                return sum + record.subTotal;
            }, 0)
        );
    },
);

export const jobOrderLevelAdjustmentsTotal = createSelector(
    jobOrderUiSelector,
    (orderUi) => {
        return Object.values(orderUi.jobOrderAdjustmentsUi.entities)
            .filter(
                (item) => item!.orderItemSeqId === "_NA_" && !item!.isAdjustmentDeleted,
            )
            .reduce((sum: any, record: any) => {
                return sum + record.amount;
            }, 0);
    },
);

// create a selector to calculate the order payments total
export const jobOrderPaymentsTotal = createSelector(
    jobOrderUiSelector,
    (orderUi) => {
        return Object.values(orderUi.jobOrderPaymentsUi.entities).reduce(
            (sum: any, record: any) => {
                return sum + record.amount;
            },
            0,
        );
    },
);
