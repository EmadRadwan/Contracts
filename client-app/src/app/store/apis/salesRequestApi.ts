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

    tagTypes: [
        "SalesRequest",           // For individual sales requests + list
        "SalesRequestInstallments", // Specific to installments per SR
        "ReserveRequest",         // For reserve requests
        "SalesRequestChequeablePayments", // Installments/deposit still pending a cheque
    ],

    endpoints(builder) {
        return {
            // -----------------------------------------------------------------
            // LIST – OData paging, sorting, filtering (Kendo Grid)
            // -----------------------------------------------------------------
            fetchSalesRequests: builder.query<ListResponse<SalesRequest>, State>({
                query: (dataState) => {
                    // Kendo's OData serializer always emits a full UTC datetime literal
                    // (e.g. "2026-08-09T00:00:00.000Z") for date-type filters, which OData
                    // binds as Edm.DateTimeOffset. saleDate is DateOnly? (Edm.Date) on the
                    // backend, so EF Core throws "binary operator Equal is not defined for
                    // DateOnly and DateTimeOffset". Strip the time/offset to produce a valid
                    // Edm.Date literal instead.
                    const odata = toODataString(dataState).replace(
                        /(saleDate\s+(?:eq|ne|gt|ge|lt|le)\s+)(\d{4}-\d{2}-\d{2})T[\d:.]+Z/g,
                        "$1$2"
                    );
                    // Backend expects $count=true for total
                    const url = `/odata/salesRequestRecords?$count=true&${odata}`;
                    return { url, method: "GET" };
                },
                providesTags: ["SalesRequest"],

                transformResponse: (response: any, meta) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    console.log("Total Sales Requests:", totalCount);
                    return {
                        data: response,
                        totalCount: totalCount,
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
                invalidatesTags: ["SalesRequest", "SalesRequestInstallments"],
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
                invalidatesTags: ["SalesRequest", "SalesRequestInstallments"],
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
                invalidatesTags: ['SalesRequest'],
            }),
            resetSalesRequest: builder.mutation<
                CreateSalesRequest.SalesRequestResponseDto,
                string
                >({
                query: (salesRequestId) => ({
                    url: `salesRequests/${salesRequestId}/reset`,
                    method: 'POST',
                }),
                invalidatesTags: ['SalesRequest'],
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
            getSalesRequestInstallments: builder.query<
                Array<{
                    installmentNumber: number;
                    dueDate: string; // YYYY-MM-DD
                    amount: number;
                    isAdvance: boolean;
                }>,
                string // salesRequestId
                >({
                query: (salesRequestId) => `salesRequests/${salesRequestId}/installments`,
                providesTags: ["SalesRequestInstallments"],
            }),

            fetchSalesRequestsByDateRange: builder.query<
                SalesRequest[],
                { fromDate: string; toDate: string }
            >({
                query: ({ fromDate, toDate }) => ({
                    url: `/salesRequests/by-date-range`,
                    method: 'GET',
                    params: { fromDate, toDate },
                }),
                providesTags: ['SalesRequest'],
            }),

            // Same as above, plus one synthesized row per APARTMENT product that has no
            // SalesRequest at all (current available/reserved inventory), tagged isSold: false.
            fetchSalesRequestsAndAvailableApartmentsByDateRange: builder.query<
                any[],
                { fromDate: string; toDate: string }
            >({
                query: ({ fromDate, toDate }) => ({
                    url: `/salesRequests/by-date-range-with-available-apartments`,
                    method: 'GET',
                    params: { fromDate, toDate },
                }),
                providesTags: ['SalesRequest'],
            }),

            // -----------------------------------------------------------------
            // CHEQUES – attach received-cheque details to not-yet-collected Payments
            // -----------------------------------------------------------------
            getChequeablePayments: builder.query<ChequeablePaymentDto[], string>({
                query: (salesRequestId) => `salesRequests/${salesRequestId}/chequeable-payments`,
                providesTags: ["SalesRequestChequeablePayments"],
            }),
            recordSalesRequestCheques: builder.mutation<
                ChequeablePaymentDto[],
                { salesRequestId: string; customerBankName: string; cheques: ChequeEntryDto[] }
                >({
                query: ({ salesRequestId, customerBankName, cheques }) => ({
                    url: `salesRequests/${salesRequestId}/cheques`,
                    method: "POST",
                    body: { salesRequestId, customerBankName, cheques },
                }),
                invalidatesTags: ["SalesRequestChequeablePayments"],
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
    useResetSalesRequestMutation,
    useCalculateInstallmentPriceMutation,
    useFetchReserveRequestsQuery,
    useAddReserveRequestMutation,
    useUpdateReserveRequestMutation, useGetSalesRequestInstallmentsQuery,
    useLazyFetchSalesRequestsByDateRangeQuery,
    useLazyFetchSalesRequestsAndAvailableApartmentsByDateRangeQuery,
    useGetChequeablePaymentsQuery,
    useRecordSalesRequestChequesMutation,
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



export interface ChequeablePaymentDto {
    paymentId: string;
    paymentTypeId?: string | null;
    dueDate?: string | null; // YYYY-MM-DD
    amount: number;
    comments?: string | null;
}

export interface ChequeEntryDto {
    paymentId: string;
    paymentMethodId: string;
    chequeNumber: string;
    chequeDate: string; // YYYY-MM-DD
    amount: number;
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

