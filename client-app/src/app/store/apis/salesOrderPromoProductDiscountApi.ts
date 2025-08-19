import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {ProcessOrderItemResult} from "../../models/order/ProcessOrderItemResult";

const salesOrderPromoProductDiscountApi = createApi({
    reducerPath: "salesOrderPromoProductDiscount",
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
            fetchSalesOrderPromoProductDiscount: builder.query<ProcessOrderItemResult, any>({
                query: (orderItem) => {
                    return {
                        url: "/products/calculateOrderItemPromoProductDiscount",
                        body: {...orderItem},
                        method: "POST",
                    };
                },
            }),
        };
    },
});

export const {
    useFetchSalesOrderPromoProductDiscountQuery,
    endpoints: salesOrderPromoProductDiscountEndpoints,
} = salesOrderPromoProductDiscountApi;
export {salesOrderPromoProductDiscountApi};
