import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {State, toODataString} from '@progress/kendo-data-query';
// import {Payment} from "../../../models/accounting/payment";
import {store} from "../configureStore";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const productStoresApi = createApi({
    reducerPath: "productStores",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            return headers;
        },
    }),
    tagTypes: ["ProductStores"],
    endpoints(builder) {
        return {
            fetchProductStores: builder.query<ListResponse<any>, State>({
                providesTags: ["ProductStores"],
                query: (queryArgs) => {
                    const url = `/odata/productStoreRecords?count=true&${toODataString(queryArgs)}`
                    return {
                        url,
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                }
            }),
            fetchProductStoreForLoggedInUser: builder.query<any, any>({
                query: () => {
                    return {
                        url: '/productStores/getProductStoreForLoggedInUser',
                        method: "GET"
                    }
                }
            })
        }
    }
});

export const {
    useFetchProductStoresQuery,
    useFetchProductStoreForLoggedInUserQuery
} = productStoresApi;
export {productStoresApi};
