import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {Quote} from "../../../models/order/quote";
import {State, toODataString} from "@progress/kendo-data-query";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const quotesApi = createApi({
    reducerPath: "quotes",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            console.log("getState", getState());
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            return headers;
        },
    }),

    tagTypes: ["Quotes"],
    endpoints(builder) {
        return {
            fetchQuotes: builder.query<ListResponse<Quote>, State>({
                query: (queryArgs) => {
                    const url = `/odata/QuoteRecords?$count=true&${toODataString(
                        queryArgs
                    )}`;
                    return { url, method: "GET" };
                },
                providesTags: ["Quotes"],
                transformResponse: (response: any, meta, arg) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            createQuote: builder.mutation({
                query: (quote) => {
                    return {
                        url: "/quotes/createQuote",
                        method: "POST",
                        body: {...quote},
                    };
                },
                invalidatesTags: ["Quotes"],
            }),
            addOrderFromQuote: builder.mutation({
                query: (quote) => {
                    return {
                        url: "/jobOrders/createJobOrderFromQuote",
                        method: "POST",
                        body: {...quote},
                    };
                },
            }),
            updateQuote: builder.mutation({
                query: (quote) => {
                    return {
                        url: "/quotes/updateOrApproveQuote",
                        method: "PUT",
                        body: {...quote},
                    };
                },
                invalidatesTags: ["Quotes"],
            }),
        };
    },
});

export const {
    useFetchQuotesQuery,
    useCreateQuoteMutation,
    useUpdateQuoteMutation,
    useAddOrderFromQuoteMutation,
} = quotesApi;
export {quotesApi};
