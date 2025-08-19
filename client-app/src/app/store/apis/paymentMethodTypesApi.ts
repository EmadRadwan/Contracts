import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {PaymentMethodType} from "../../models/accounting/paymentMethodType";
import {State, toODataString} from '@progress/kendo-data-query';

interface ListResponse<T> {
    data: T[]
    count: number
}

const paymentMethodTypesApi = createApi({
    reducerPath: "paymentMethodTypes",
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
            fetchPaymentMethodTypes: builder.query<ListResponse<PaymentMethodType>, State>({
                query: (queryArgs) => {
                    const url = `/odata/paymentMethodTypeRecords?count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"}
                },
                transformResponse: (response: any, meta) => {
                    // console.log(response)
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            fetchAllPaymentMethodTypes: builder.query<PaymentMethodType[], any>({
                query: () => {
                  return {
                    url: "/paymentMethodTypes/getPaymentMethodTypes",
                    method: "GET",
                  };
                }
              }),
        };
    },
});

export const {useFetchPaymentMethodTypesQuery, useFetchAllPaymentMethodTypesQuery} = paymentMethodTypesApi;
export {paymentMethodTypesApi};
