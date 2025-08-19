import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { OrderItem } from "../../../app/models/order/orderItem";
import { OrderAdjustment } from "../../../app/models/order/orderAdjustment";
import { store } from "../../configureStore";
import {setUiOrderAdjustments, setUiTaxAdjustments} from "../../../../features/orders/slice/orderAdjustmentsUiSlice";

export interface TaxCalculationPayload {
    OrderItems: OrderItem[];
    OrderAdjustments: OrderAdjustment[];
}

export interface TaxCalculationResult {
    adjustments: OrderAdjustment[];
}

export const taxApi = createApi({
    reducerPath: "taxApi",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers) => {
            const token = store.getState().account.user?.token;
            console.log("DEBUG: Authorization Token:", token);
            console.log("DEBUG: API Base URL:", import.meta.env.VITE_API_URL);
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            return headers;
        },
    }),
    endpoints: (build) => ({
        calculateTax: build.mutation<OrderAdjustment[], TaxCalculationPayload>({
            query: (payload) => {
                console.log("DEBUG: Sending request to:", import.meta.env.VITE_API_URL + "/taxes/calculateTax");
                console.log("DEBUG: Method:", "POST");
                console.log("DEBUG: Payload:", JSON.stringify(payload, null, 2));
                return {
                    url: "/taxes/calculateTax",
                    method: "POST",
                    body: payload,
                };
            },
        }),
        recalculateTaxes: build.mutation<void, { orderItems: OrderItem[]; forceCalculate?: boolean }>({
            async queryFn({ orderItems, forceCalculate = false }, { dispatch, getState }) {
                const state: any = getState();
                const addTax = state.sharedOrderUi.addTax;

                if ((!addTax && !forceCalculate) || !orderItems || orderItems.length === 0) {
                    dispatch(setUiTaxAdjustments([]));
                    return { data: undefined };
                }

                try {
                    const payload: TaxCalculationPayload = {
                        OrderItems: orderItems.map(item => ({
                            ...item,
                            collectTax: item.collectTax ?? false,
                        })),
                        OrderAdjustments: Object.values(
                            state.orderAdjustmentsUi.orderAdjustments.entities
                        ).filter(
                            (adj) =>
                                (adj.orderAdjustmentTypeId === "PROMOTION_ADJUSTMENT" ||
                                    adj.orderAdjustmentTypeId === "DISCOUNT_ADJUSTMENT") &&
                                adj.isAdjustmentDeleted === false
                        ),
                    };
                    const taxResult = await dispatch(
                        taxApi.endpoints.calculateTax.initiate(payload)
                    ).unwrap();

                    dispatch(setUiTaxAdjustments(taxResult || []));
                    return { data: undefined };
                } catch (error: any) {
                    console.error("Failed to recalculate tax:", error);
                    throw error;
                }
            },
        }),
    }),

});

export const {  useRecalculateTaxesMutation } = taxApi;