import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../configureStore";
import { State, toODataString } from "@progress/kendo-data-query";
import {SalesRequest} from "../../models/order/SalesRequest";
import {ReserveRequest} from "../../models/order/ReserveRequest";

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
            
        fetchReserveRequests: builder.query<ListResponse<ReserveRequest>, State>({
                query: (dataState) => {
                    const odata = toODataString(dataState);
                    // Backend expects $count=true for total
                    const url = `/odata/reserveRequestRecords?$count=true&${odata}`;
                    return { url, method: "GET" };
                },
                providesTags: ["ReserveRequest"],

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
            addReserveRequest: builder.mutation<string, any>({
                query: (payload) => ({
                    url: "/reserveRequests/reserve",
                    method: "POST",
                    body: payload,
                }),
                invalidatesTags: ["ReserveRequest"],
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
            updateReserveRequest: builder.mutation<ReserveRequestResponseDto, { reserveRequestDto: ReserveRequestDto }>({
                query: (payload) => ({
                    url: "reserveRequests",        // ← simplified, no ID in URL
                    method: "PUT",
                    body: payload,
                }),
                invalidatesTags: ["ReserveRequest"],
            }),
            // In your salesRequestApi.ts
            approveSalesRequest: builder.mutation<
                CreateSalesRequest.SalesRequestResponseDto,
                string
                >({
                query: (salesRequestId) => ({
                    url: `salesRequests/${salesRequestId}/approve`,
                    method: 'POST',
                }),
                invalidatesTags: (result, error, id) => [
                    { type: 'SalesRequest', id },
                    { type: 'SalesRequest', id: 'LIST' },
                ],
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
            calculateInstallmentPrice: builder.mutation<
                CalculatorResponse,
                CalculateInstallmentPriceRequest
                >({
                query: (body) => ({
                    url: "/salesRequests/calculate-meter-price",
                    method: "POST",
                    body,
                }),
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
    useApproveSalesRequestMutation,
    useCalculateInstallmentPriceMutation,
    useFetchReserveRequestsQuery,
    useAddReserveRequestMutation,
    useUpdateReserveRequestMutation,
} = salesRequestApi;

export { salesRequestApi };


export interface CalculateInstallmentPriceRequest {
    cashPricePerM2: number;
    annualDiscountRate: number;
    downPaymentPercentage: number;
    durationYears: number;
    installmentsPerYear: number;
}

export interface InstallmentCalcResult {
    period: number;
    dueDate: string;
    amount: number;
    presentValue: number;
}



interface CalculatorResponse {
    cashPricePerM2: number;
    installmentPricePerM2: number;
    quarterlyInstallmentPerM2?: number;      // ← NEW – comes from backend
    totalInstallments: number;
    downPaymentPercentage: number;
    annualDiscountRate: number;
    pvaf: number;
    increasePercentage: number;
    installmentsPerYear?: number;            // ← optional, for dynamic label
    schedule: InstallmentCalcResult[];
}

