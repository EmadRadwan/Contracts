import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {OrderAdjustment} from "../../models/order/orderAdjustment";
import {OrderItem} from "../../models/order/orderItem";

const salesOrderTaxAdjustmentsApi = createApi({
    reducerPath: "salesOrderTaxAdjustments",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            return headers;
        },
    }),

    endpoints(builder) {
        return {
            fetchSalesOrderTaxAdjustments: builder.query<OrderAdjustment[],
                {
                    orderItems: OrderItem[] | undefined;
                    orderAdjustments: OrderAdjustment[] | undefined;
                }>({
                query: (orderItemsAndAdjustments) => {
                    console.log(
                        "orderItemsAndAdjustments from Tax Api",
                        orderItemsAndAdjustments,
                    );
                    return {
                        url: "/taxes/calculateTaxAdjustmentsForSalesOrder",
                        body: {
                            orderItems: orderItemsAndAdjustments.orderItems,
                            orderAdjustments: orderItemsAndAdjustments.orderAdjustments,
                        },
                        method: "POST",
                    };
                },
                transformResponse: (response: any, meta) => {
                    console.log("orderAdjustments from Tax Api", response);
                    return response as OrderAdjustment[];
                },
            }),
        };
    },
});

export const {
    useFetchSalesOrderTaxAdjustmentsQuery,
    endpoints: salesOrderTaxAdjustmentsEndpoints,
} = salesOrderTaxAdjustmentsApi;
export {salesOrderTaxAdjustmentsApi};
