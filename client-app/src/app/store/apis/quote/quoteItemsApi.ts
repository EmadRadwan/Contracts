import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {QuoteItem} from "../../../models/order/quoteItem";
import {ProductLov} from "../../../models/product/productLov";
import {store} from "../../configureStore";
import {setUiQuoteItemsFromApi} from "../../../../features/orders/slice/quoteItemsUiSlice";

interface ListProductLov<T> {
    data: T[];
}

const quoteItemsApi = createApi({
    reducerPath: "quoteItems",
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

    tagTypes: ["QuoteItems"],
    endpoints(builder) {
        return {
            fetchQuoteItems: builder.query<QuoteItem[], any>({
                query: (quoteId) => {
                    return {
                        url: `/quotes/${quoteId}/getQuoteItems`,
                        params: quoteId,
                        method: "GET",
                    };
                },
                keepUnusedDataFor: 1,
                async onQueryStarted(id, {dispatch, queryFulfilled}) {
                    // `onStart` side-effect
                    try {
                        const {data} = await queryFulfilled;
                        console.log("apiData", data);
                        dispatch(setUiQuoteItemsFromApi(data));
                    } catch (err) {
                        // `onError` side-effect
                        //dispatch(messageCreated('Error fetching post!'))
                    }
                },
                //providesTags: ['QuoteItems'],
                transformResponse: (response: any, meta, arg) => {
                    console.log(response);
                    return response as QuoteItem[];
                },
            }),
            fetchQuoteItemProduct: builder.query<ListProductLov<ProductLov>, any>({
                query: (quoteItem) => {
                    const quoteItemId = quoteItem.quoteId + quoteItem.quoteItemSeqId;
                    console.count("fetchQuoteItemProduct");
                    return {
                        url: `/quotes/${quoteItemId}/getQuoteItemProduct`,
                        params: quoteItemId,
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta, arg) => {
                    return {
                        data: response,
                    };
                },
            }),
            addQuoteItems: builder.mutation({
                query: (quoteItems) => {
                    return {
                        url: "/quotes/createQuoteItems",
                        method: "POST",
                        body: {...quoteItems},
                    };
                },
                //invalidatesTags: ['QuoteItems'],
            }),
            updateQuoteItems: builder.mutation({
                query: (quoteItems) => {
                    return {
                        url: "/quotes/updateOrApproveQuoteItems",
                        method: "PUT",
                        body: {...quoteItems},
                    };
                },
            }),
            approveQuoteItems: builder.mutation({
                query: (quoteItems) => {
                    return {
                        url: "/quotes/updateOrApproveQuoteItems",
                        method: "PUT",
                        body: {...quoteItems},
                    };
                },
            }),
        };
    },
});

export const {
    useFetchQuoteItemsQuery,
    useFetchQuoteItemProductQuery,
    useAddQuoteItemsMutation,
    useUpdateQuoteItemsMutation,
    useApproveQuoteItemsMutation,
} = quoteItemsApi;
export {quoteItemsApi};
