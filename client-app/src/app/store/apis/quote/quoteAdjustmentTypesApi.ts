import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {QuoteAdjustmentType} from "../../../models/order/quoteAdjustmentType";
import {store} from "../../configureStore";

const quoteAdjustmentTypesApi = createApi({
    reducerPath: "quoteAdjustmentTypes",
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
            fetchQuoteAdjustmentTypes: builder.query<QuoteAdjustmentType[],
                undefined>({
                query: () => {
                    return {
                        url: "/quoteAdjustmentTypes",
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta) => {
                    //console.log("quoteAdjustmentTypesData from Api",response)
                    return response as QuoteAdjustmentType[];
                },
            }),
        };
    },
});

export const {useFetchQuoteAdjustmentTypesQuery} = quoteAdjustmentTypesApi;
export {quoteAdjustmentTypesApi};
