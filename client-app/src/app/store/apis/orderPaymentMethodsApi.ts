import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";

interface ListResponse<T> {
    currentPage: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    data: T[];
}

const orderPaymentMethodsApi = createApi({
    reducerPath: "orderPaymentMethods",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            const lang = store.getState().localization.language;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            if (lang) { 
                headers.set('Accept-Language', lang)
            }
            return headers;
        },
    }),

    endpoints(builder) {
        return {
            fetchOrderPaymentMethods: builder.query<any, any | undefined>({
                query: () => {
                    return {
                        url: "/productStores/getProductStorePaymentMethods",
                        method: "GET",
                    };
                },
                transformResponse: (res, meta, arg) => {
                    return res
                }
            }),
        };
    },
});

export const {useFetchOrderPaymentMethodsQuery} = orderPaymentMethodsApi;

export {orderPaymentMethodsApi};
