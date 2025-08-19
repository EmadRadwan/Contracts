import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {QuoteAdjustment} from "../../../models/order/quoteAdjustment";
import {store} from "../../configureStore";
import {setUiQuoteAdjustmentsFromApi} from "../../../../features/orders/slice/quoteAdjustmentsUiSlice";

const quoteAdjustmentsApi = createApi({
    reducerPath: "quoteAdjustments",
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
            fetchQuoteAdjustments: builder.query<QuoteAdjustment[], any>({
                query: (quoteId) => {
                    console.log("queryArgs quote adjustment", quoteId);
                    return {
                        url: `/quotes/${quoteId}/getQuoteAdjustments`,
                        params: quoteId,
                        method: "GET",
                    };
                },
                keepUnusedDataFor: 1,
                async onQueryStarted(id, {dispatch, queryFulfilled}) {
                    // `onStart` side-effect
                    try {
                        const {data} = await queryFulfilled;
                        // `onSuccess` side-effect
                        dispatch(setUiQuoteAdjustmentsFromApi(data));
                    } catch (err) {
                        // `onError` side-effect
                        //dispatch(messageCreated('Error fetching post!'))
                    }
                },
                transformResponse: (response: any, meta) => {
                    return response as QuoteAdjustment[];
                },
            }),
            addQuoteAdjustments: builder.mutation({
                query: (quoteAdjustments) => {
                    return {
                        url: "/quotes/createQuoteAdjustments",
                        method: "POST",
                        body: {...quoteAdjustments},
                    };
                },
            }),
            updateQuoteAdjustments: builder.mutation({
                query: (quoteAdjustments) => {
                    return {
                        url: "/quotes/updateOrApproveQuoteAdjustments",
                        method: "PUT",
                        body: {...quoteAdjustments},
                    };
                },
            }),
            approveQuoteAdjustments: builder.mutation({
                query: (quoteAdjustments) => {
                    return {
                        url: "/quotes/updateOrApproveQuoteAdjustments",
                        method: "PUT",
                        body: {...quoteAdjustments},
                    };
                },
            }),
        };
    },
});

export const {
    useFetchQuoteAdjustmentsQuery,
    useAddQuoteAdjustmentsMutation,
    useUpdateQuoteAdjustmentsMutation,
    useApproveQuoteAdjustmentsMutation,
} = quoteAdjustmentsApi;
export {quoteAdjustmentsApi};
