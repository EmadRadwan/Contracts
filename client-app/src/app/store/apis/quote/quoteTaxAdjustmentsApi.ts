import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {QuoteAdjustment} from "../../../models/order/quoteAdjustment";
import {QuoteItem} from "../../../models/order/quoteItem";
import {store} from "../../configureStore";

const quoteTaxAdjustmentsApi = createApi({
    reducerPath: "quoteTaxAdjustments",
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
            fetchQuoteTaxAdjustments: builder.query<QuoteAdjustment[],
                {
                    quoteItems: QuoteItem[] | undefined;
                    quoteAdjustments: QuoteAdjustment[] | undefined;
                }>({
                query: (quoteItemsAndAdjustments) => {
                    console.log(
                        "quoteItemsAndAdjustments from Tax Api",
                        quoteItemsAndAdjustments,
                    );
                    return {
                        url: "/taxes/calculateTaxAdjustmentsForQuote",
                        body: {
                            quoteItems: quoteItemsAndAdjustments.quoteItems,
                            quoteAdjustments: quoteItemsAndAdjustments.quoteAdjustments,
                        },
                        method: "POST",
                    };
                },
                transformResponse: (response: any, meta) => {
                    console.log("quoteAdjustments from Tax Api", response);
                    return response as QuoteAdjustment[];
                },
            }),
        };
    },
});

export const {
    useFetchQuoteTaxAdjustmentsQuery,
    endpoints: quoteTaxAdjustmentsEndpoints,
} = quoteTaxAdjustmentsApi;
export {quoteTaxAdjustmentsApi};
