import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {State, toODataString} from "@progress/kendo-data-query";
import {store} from "../../configureStore";
import {BillOfMaterial} from "../../../models/manufacturing/billOfMaterial";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const productsWithBOMApi = createApi({
    reducerPath: "productsWithBOM",
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
            fetchProductsWithBOM: builder.query<ListResponse<BillOfMaterial>,
                State>({
                query: (queryArgs) => {
                    const url = `/odata/billOfMaterialRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                transformResponse: (response: any, meta, arg) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    console.log("response", response);
                    console.log("totalCount", totalCount);
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            })
        };
    },
});

export const {
    useFetchProductsWithBOMQuery,
} = productsWithBOMApi;
export {productsWithBOMApi};
