import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {OrderAdjustmentType} from "../../models/order/orderAdjustmentType";

const orderAdjustmentTypesApi = createApi({
    reducerPath: "orderAdjustmentTypes",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            const lang = store.getState().localization.language
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            if (lang) {
                headers.set("Accept-Language", lang)
            }
            return headers;
        },
    }),

    endpoints(builder) {
        return {
            fetchOrderAdjustmentTypes: builder.query<OrderAdjustmentType[],
                undefined>({
                query: () => {
                    return {
                        url: "/orderAdjustmentTypes",
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta) => {
                    //console.log("orderAdjustmentTypesData from Api",response)
                    return response as OrderAdjustmentType[];
                },
            }),
        };
    },
});

export const {useFetchOrderAdjustmentTypesQuery} = orderAdjustmentTypesApi;
export {orderAdjustmentTypesApi};
