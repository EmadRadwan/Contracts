import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {State, toODataString} from "@progress/kendo-data-query";
import {store} from "../../configureStore";
import {BOMProductComponent} from "../../../models/manufacturing/bOMProductComponent";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const bomProductComponentsApi = createApi({
    reducerPath: "bomProductComponents",
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
            fetchBomProductComponentsApi: builder.query<ListResponse<BOMProductComponent>,
                State>({
                query: (queryArgs) => {
                    const url = `/odata/bOMProductComponentRecords?$count=true&${toODataString(queryArgs)}`;
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
    useFetchBomProductComponentsApiQuery,
} = bomProductComponentsApi;
export {bomProductComponentsApi};
