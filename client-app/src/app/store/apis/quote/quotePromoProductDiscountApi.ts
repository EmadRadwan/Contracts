import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {ProcessOrderItemResult} from "../../../models/order/ProcessOrderItemResult";
import {store} from "../../configureStore";

const quotePromoProductDiscountApi = createApi({
    reducerPath: "quotePromoProductDiscount",
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
            fetchQuotePromoProductDiscount: builder.query<ProcessOrderItemResult, any>({
                query: (quoteItem) => {
                    return {
                        url: "/products/calculateQuoteItemPromoProductDiscount",
                        body: {...quoteItem},
                        method: "POST",
                    };
                },
            }),
        };
    },
});

export const {
    useFetchQuotePromoProductDiscountQuery,
    endpoints: quotePromoProductDiscountEndpoints,
} = quotePromoProductDiscountApi;
export {quotePromoProductDiscountApi};
