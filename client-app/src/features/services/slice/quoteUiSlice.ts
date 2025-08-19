import {createEntityAdapter, createSelector, createSlice, EntityState, PayloadAction,} from "@reduxjs/toolkit";
import {QuoteAdjustment} from "../../../app/models/order/quoteAdjustment";
import {QuoteItem} from "../../../app/models/order/quoteItem";
import {RootState} from "../../../app/store/configureStore";
import {Vehicle} from "../../../app/models/service/vehicle";

interface QuoteUiState {
    selectedCustomerId: string | undefined;
    selectProductOrService: string | undefined;
    selectedProductId: string | undefined;
    selectedProduct: any | undefined;
    selectedVehicleId: string | undefined;
    selectedVehicle: Vehicle | undefined;
    selectedQuoteItem: QuoteItem | undefined;
    quoteAdjustmentsUi: EntityState<QuoteAdjustment>;
    quoteItemsUi: EntityState<QuoteItem>;
    relatedRecords: QuoteItem[] | undefined;
    isSelectedServicePriceZero: boolean;
    isNewServiceRateAndSpecificationAdded: boolean;
}

function calculateQuoteItemAdjustments(
    items: QuoteItem[],
    adjustments: QuoteAdjustment[],
): QuoteItem[] {
    console.log(
        "%c calculateQuoteItemAdjustments:",
        "color: red; font-weight: bold;",
    );

    const adjustedItems = items.map((quoteItem: QuoteItem) => {
        const quoteItemManualAdjustments = calculateAdjustments(
            quoteItem,
            true,
            adjustments,
        );
        const quoteItemAutoAdjustments = calculateAdjustments(
            quoteItem,
            false,
            adjustments,
        );

        const calculateProperties = (subtotalWithoutAdjustments: number) => {
            return {
                // Adjusted properties
                totalItemAdjustmentsManual: quoteItemManualAdjustments,
                totalItemAdjustmentsAuto: quoteItemAutoAdjustments,
                totalItemAdjustments:
                    quoteItemManualAdjustments + quoteItemAutoAdjustments,
                subTotal:
                    subtotalWithoutAdjustments +
                    quoteItemManualAdjustments +
                    quoteItemAutoAdjustments,
                quoteUnitPrice:
                    (subtotalWithoutAdjustments +
                        quoteItemManualAdjustments +
                        quoteItemAutoAdjustments) /
                    quoteItem.quantity,
            };
        };

        const subTotalWithoutAdjustments = quoteItem.quoteUnitListPrice
            ? quoteItem.quoteUnitListPrice * quoteItem.quantity
            : quoteItem.quoteUnitPrice * quoteItem.quantity;

        return {
            ...quoteItem,
            ...calculateProperties(subTotalWithoutAdjustments),
        };
    });
    console.log("adjustedItems", adjustedItems)
    return adjustedItems;
}

function calculateAdjustments(
    quoteItem: QuoteItem,
    isManual: boolean,
    adjustments: QuoteAdjustment[],
): number {
    console.log(
        "%c calculatedAjustments:",
        "color: blue; font-weight: bold;",
        quoteItem,
        adjustments,
    );

    // Filter adjustments based on the provided criteria
    const filteredAdjustments = adjustments.filter(
        (adjustment: QuoteAdjustment) => {
            return (
                adjustment.quoteItemSeqId !== "_NA_" &&
                !adjustment.isAdjustmentDeleted &&
                adjustment.isManual === (isManual ? "Y" : "N") &&
                //adjustment.correspondingProductId === quoteItem.productId &&
                adjustment.quoteItemSeqId === quoteItem.quoteItemSeqId
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
        (sum: number, record: QuoteAdjustment) => {
            const amount = record.amount || 0; // default to 0 if record.amount is null or undefined
            const quantity = quoteItem.quantity || 1; // default to 1 if quoteItem.quantity is null or undefined
            return sum + (isManual ? amount * quantity : amount);
        },
        0,
    );

    return adjustmentsTotal;
}

const quoteItemsAdapter = createEntityAdapter<QuoteItem>({
    selectId: (QuoteItem) => QuoteItem.quoteId!.concat(QuoteItem.quoteItemSeqId),
});

const quoteAdjustmentsAdapter = createEntityAdapter<QuoteAdjustment>({
    selectId: (QuoteAdjustment) => QuoteAdjustment.quoteAdjustmentId,
});

export const initialState: QuoteUiState = {
    selectedCustomerId: undefined,
    selectProductOrService: undefined,
    selectedProductId: undefined,
    selectedProduct: undefined,
    selectedVehicleId: undefined,
    selectedVehicle: undefined,
    selectedQuoteItem: undefined,
    quoteAdjustmentsUi: quoteAdjustmentsAdapter.getInitialState(),
    quoteItemsUi: quoteItemsAdapter.getInitialState(),
    relatedRecords: undefined,
    isSelectedServicePriceZero: false,
    isNewServiceRateAndSpecificationAdded: false,
};

export const quoteUiSlice = createSlice({
    name: "quoteUi",
    initialState: initialState,
    reducers: {
        resetUiQuote: (state, action) => {
            state.quoteItemsUi = quoteItemsAdapter.getInitialState();
            state.quoteAdjustmentsUi = quoteAdjustmentsAdapter.getInitialState();
            state.selectedCustomerId = undefined;
            state.selectedQuoteItem = undefined;
            state.selectedProductId = undefined;
            state.selectedProduct = undefined;
        },
        setUiQuoteItems: (state, action) => {
            if (action.payload.length === 0) {
                state.quoteItemsUi = quoteItemsAdapter.getInitialState();
            } else {
                state.quoteItemsUi = quoteItemsAdapter.upsertMany(
                    state.quoteItemsUi,
                    action.payload,
                );
            }
        },
        setUiQuoteAdjustments: (state, action) => {
            if (action.payload.length === 0) {
                state.quoteAdjustmentsUi = quoteAdjustmentsAdapter.getInitialState();
            } else {
                console.log(
                    "%c setUiQuoteAdjustments:",
                    "color: red; font-weight: bold;",
                    action.payload,
                );
                quoteAdjustmentsAdapter.upsertMany(
                    state.quoteAdjustmentsUi,
                    action.payload,
                );
            }
        },
        deletePromoQuoteItem: (state, action) => {
            if (action.payload.length > 0) {
                quoteItemsAdapter.removeOne(state.quoteItemsUi, action.payload);
            }
        },
        deletePromoQuoteAdjustments: (state, action) => {
            if (action.payload.length > 0) {
                // set isAdjustmentDeleted to true for the adjustments in the payload
                action.payload.forEach((adjustmentId: string) => {
                    state.quoteAdjustmentsUi!.entities[
                        adjustmentId
                        ]!.isAdjustmentDeleted = true;
                });
            }
        },
        setUiNewQuoteItems: (state, action) => {
            if (action.payload.length === 0) {
                state.quoteItemsUi = quoteItemsAdapter.getInitialState();
            } else {
                // Calculate adjustments only if quoteAdjustmentsUi is not empty
                state.quoteItemsUi = quoteItemsAdapter.setAll(
                    state.quoteItemsUi,
                    action.payload,
                );
            }
        },

        setUiNewQuoteAdjustments: (state, action) => {
            // Clear the state if payload is empty
            if (action.payload.length === 0) {
                state.quoteAdjustmentsUi = quoteAdjustmentsAdapter.getInitialState();
            } else {
                console.log(
                    "%c setUiNewQuoteAdjustments:",
                    "color: red; font-weight: bold;",
                    action.payload,
                );
                state.quoteAdjustmentsUi = quoteAdjustmentsAdapter.setAll(
                    state.quoteAdjustmentsUi,
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
            state.quoteItemsUi = quoteItemsAdapter.getInitialState();
            state.quoteAdjustmentsUi = quoteAdjustmentsAdapter.getInitialState();
        },
        setSelectedQuoteItem(state, action: PayloadAction<QuoteItem | undefined>) {
            console.log(
                "%c Setting Selected quote item:",
                "color: green; font-weight: bold;",
                action.payload,
            );

            state.selectedQuoteItem = action.payload;
        },
        setRelatedRecords(state, action: PayloadAction<QuoteItem[] | undefined>) {
            state.relatedRecords = action.payload;
        },
        setSelectProductOrService(
            state,
            action: PayloadAction<string | undefined>,
        ) {
            state.selectProductOrService = action.payload;
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
    setUiQuoteItems,
    setUiQuoteAdjustments,
    setSelectedQuoteItem,
    setCustomerId,
    setVehicleId,
    setSelectedVehicle,
    setProductId,
    setSelectedProduct,
    setUiNewQuoteAdjustments,
    setUiNewQuoteItems,
    resetUiQuote,
    deletePromoQuoteItem,
    deletePromoQuoteAdjustments,
    setRelatedRecords,
    setSelectProductOrService,
    setIsSelectedPriceZero,
    setIsNewServiceRateAndSpecificationAdded,
} = quoteUiSlice.actions;
export const quoteAdjustmentSelectors = quoteAdjustmentsAdapter.getSelectors(
    (state: RootState) => state.quoteUi.quoteAdjustmentsUi,
);
export const quoteItemsSelectors = quoteItemsAdapter.getSelectors(
    (state: RootState) => state.quoteUi.quoteItemsUi,
);
export const {selectAll: quoteAdjustmentsEntities} = quoteAdjustmentSelectors;
export const {selectAll: quoteItemsEntities} = quoteItemsSelectors;

const quoteUiSelector = (state: RootState) => state.quoteUi;
export const quoteItemsSelector = createSelector(quoteUiSelector, (quoteUi) => {

    return Object.values(quoteUi.quoteItemsUi.entities).filter((quoteItem) => {
        console.log("selectorTest", quoteItem)
        if (!quoteItem!.isProductDeleted) return quoteItem;
    });
});

export const quoteAdjustmentsSelector = createSelector(
    quoteUiSelector,
    (quoteUi) => {
        return Object.values(quoteUi.quoteAdjustmentsUi.entities).filter(
            (quoteAdjustment) => {
                if (!quoteAdjustment!.isAdjustmentDeleted) return quoteAdjustment;
            },
        );
    },
);

export const quoteLevelAdjustments = createSelector(
    quoteUiSelector,
    (quoteUi) => {
        return Object.values(quoteUi.quoteAdjustmentsUi.entities).filter(
            (quoteAdjustment) => {
                if (
                    quoteAdjustment!.quoteItemSeqId === "_NA_" &&
                    !quoteAdjustment!.isAdjustmentDeleted
                )
                    return quoteAdjustment;
            },
        );
    },
);

// create a selector to get the quoteItemAdjustments for the selected quote
// using the quoteAdjustmentsUi and the selectedQuoteId
// and the CreateSelector from reselect
export const quoteItemAdjustmentsSelector = createSelector(
    (state: { quoteUi: { quoteAdjustmentsUi: any } }) =>
        state.quoteUi.quoteAdjustmentsUi,
    (state: { quoteUi: { selectedQuoteItem: any } }) =>
        state.quoteUi.selectedQuoteItem,
    (state, quoteItem) => {
        console.log("%c state:", "color: red; font-weight: bold;", state);
        console.log(
            "%c selectedQuoteItem:",
            "color: blue; font-weight: bold;",
            quoteItem,
        );
        return (
            quoteItem &&
            Object.values(state.entities).filter((quoteAdjustment: any) => {
                if (
                    quoteAdjustment!.quoteId === quoteItem.quoteId &&
                    !quoteAdjustment!.isAdjustmentDeleted &&
                    quoteAdjustment.quoteItemSeqId === quoteItem.quoteItemSeqId
                )
                    return quoteAdjustment;
            })
        );
    },
);

export const quoteItemSubTotal = createSelector(
    quoteUiSelector,
    (state: { quoteUi: { selectedQuoteItem: any } }) =>
        state.quoteUi.selectedQuoteItem,
    (quoteUi) => {
        const filteredQuoteItems = Object.values(
            quoteUi.quoteItemsUi.entities,
        ).filter(
            (item) =>
                !item!.isProductDeleted &&
                item!.quoteItemSeqId === quoteUi.selectedQuoteItem?.quoteItemSeqId,
        );

        return (
            filteredQuoteItems &&
            filteredQuoteItems!.reduce((sum: any, record: any) => {
                return sum + record.subTotal;
            }, 0)
        );
    },
);

export const quoteLevelAdjustmentsTotal = createSelector(
    quoteUiSelector,
    (quoteUi) => {


        const value = Object.values(quoteUi.quoteAdjustmentsUi.entities)
            .filter(
                (item) => item!.quoteItemSeqId === "_NA_" && !item!.isAdjustmentDeleted,
            )
            .reduce((sum: any, record: any) => {
                return sum + record.amount;
            }, 0);


        return value;
    },
);

export const selectAdjustedQuoteItems = createSelector(
    [quoteItemsEntities, quoteAdjustmentsEntities],
    (quoteItems, quoteAdjustments) => {
        const calcResult = calculateQuoteItemAdjustments(
            Object.values(quoteItems) as QuoteItem[],
            Object.values(quoteAdjustments) as QuoteAdjustment[],
        );
        return calcResult.filter((quoteItem) => {
            if (!quoteItem!.isProductDeleted) return quoteItem;
        });
    },
);

export const selectAdjustedQuoteItemsWithProductId = createSelector(
    [quoteItemsEntities, quoteAdjustmentsEntities],
    (quoteItems, quoteAdjustments) => {
        const calcResult = calculateQuoteItemAdjustments(
            Object.values(quoteItems) as QuoteItem[],
            Object.values(quoteAdjustments) as QuoteAdjustment[],
        );
        return calcResult.filter((quoteItem) => {
            if (!quoteItem!.isProductDeleted) {
                quoteItem.productId = quoteItem.productId.productId;
                return quoteItem;
            }
        });
    },
);

export const quoteSubTotal = createSelector(
    selectAdjustedQuoteItems,
    (adjustedQuoteItems) => {


        if (!adjustedQuoteItems) {
            return 0;
        }

        return adjustedQuoteItems.reduce((sum, record) => {
            return sum + (record.subTotal ?? 0); // Use optional chaining and nullish coalescing
        }, 0);
    },
);
