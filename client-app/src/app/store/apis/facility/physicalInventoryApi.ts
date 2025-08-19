import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import { ProductInventoryItem } from "../../../models/facility/productInventoryItem";
import { State, toODataString } from "@progress/kendo-data-query";

interface ListResponse<T> {
    data: T[],
    total: number
}

const physicalInventoryApi = createApi({
    reducerPath: "physicalInventory",
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
            fetchPhysicalInventoryByProduct: builder.query<ListResponse<ProductInventoryItem>, {facilityId: string, productId: string, dataState: State}>({
                query: ({facilityId, productId, dataState}) => {
                    const url = `/odata/physicalInventoryRecords?$count=true&${toODataString(dataState)}&facilityId=${facilityId}&productId=${productId}`
                    return {url, method: "GET"};
                },
                transformResponse: (response: any, meta, arg) => {
                    console.log("hello", response)
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
        };
    },
});

export const {useFetchPhysicalInventoryByProductQuery} =
physicalInventoryApi;
export {physicalInventoryApi};
