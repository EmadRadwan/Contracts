import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../configureStore";
import { State, toODataString } from "@progress/kendo-data-query";
import {SalesRequest} from "../../models/order/SalesRequest";

interface ListResponse<T> {
    data: T[];
    total: number;
}

/**
 * RTK-Query API slice for SalesRequest (Apartment sales requests)
 * -----------------------------------------------------------------
 * - Uses the same baseQuery + header preparation as the ordersApi
 * - OData paging/filtering with Kendo `State`
 * - Mutations invalidate the list tag
 * - All endpoints follow the exact pattern of `ordersApi`
 */
const salesRequestApi = createApi({
    reducerPath: "salesRequests",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, { getState }) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            const lang = store.getState().localization.language;

            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            if (lang) {
                headers.set("Accept-Language", lang);
            }
            return headers;
        },
    }),

    tagTypes: ["SalesRequest"],

    endpoints(builder) {
        return {
            // -----------------------------------------------------------------
            // LIST – OData paging, sorting, filtering (Kendo Grid)
            // -----------------------------------------------------------------
            fetchSalesRequests: builder.query<ListResponse<SalesRequest>, State>({
                query: (dataState) => {
                    const odata = toODataString(dataState);
                    // Backend expects $count=true for total
                    const url = `/odata/salesRequestRecords?$count=true&${odata}`;
                    return { url, method: "GET" };
                },
                providesTags: ["SalesRequest"],

                transformResponse: (response: any, meta) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),

            // -----------------------------------------------------------------
            // CREATE
            // -----------------------------------------------------------------
            addSalesRequest: builder.mutation<string, any>({
                query: (payload) => ({
                    url: "/salesRequests",
                    method: "POST",
                    body: payload,
                }),
                invalidatesTags: ["SalesRequest"],
            }),
            // -----------------------------------------------------------------
            // UPDATE
            // -----------------------------------------------------------------
            updateSalesRequest: builder.mutation<SalesRequestResponseDto, any>({
                query: (payload) => ({
                    url: `/salesRequests/${payload.salesRequestDto.salesRequestId}`,
                    method: "PUT",
                    body: payload,
                }),
                invalidatesTags: ["SalesRequest"],
            }),
            // -----------------------------------------------------------------
            // DELETE – optional (not used in current UI but kept for completeness)
            // -----------------------------------------------------------------
            deleteSalesRequest: builder.mutation<void, string>({
                query: (salesRequestId) => ({
                    url: `/salesRequests/${salesRequestId}`,
                    method: "DELETE",
                }),
                invalidatesTags: ["SalesRequest"],
            }),
        };
    },
});

/**
 * Exported hooks – match naming convention from ordersApi
 */
export const {
    useFetchSalesRequestsQuery,
    useAddSalesRequestMutation,
    useUpdateSalesRequestMutation,
    useDeleteSalesRequestMutation,
} = salesRequestApi;

export { salesRequestApi };