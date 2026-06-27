import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {State, toODataString} from '@progress/kendo-data-query';
import {Payment} from "../../../models/accounting/payment";
import {store} from "../../configureStore";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const paymentsApi = createApi({
    reducerPath: "payments",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            const lang = store.getState().localization.language ;
            if (lang) {
                headers.set("Accept-Language", `${lang}`);
            }
            return headers;
        },
    }),
    tagTypes: ["Payments", "PaymentApplications", "NotAppliedInvoices"],
    refetchOnMountOrArgChange: true,
    endpoints(builder) {
        return {
            fetchPayments: builder.query<ListResponse<Payment>, PaymentQueryArgs>({
                providesTags: ['Payments'],
                query: (queryArgs) => {
                    // REFACTOR: Include paymentType in the query string if provided
                    const baseUrl = `/odata/paymentRecords?count=true&${toODataString(queryArgs)}`;
                    const paymentTypeParam = queryArgs.paymentType ? `&paymentType=${queryArgs.paymentType}` : '';
                    return {
                        url: `${baseUrl}${paymentTypeParam}`,
                        method: 'GET',
                    };
                },
                transformResponse: (response: any, meta, arg) => {
                    const { totalCount } = JSON.parse(meta!.response!.headers.get('count')!);
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            fetchPaymentsWithDueStatus: builder.query<ListResponse<PaymentWithDueStatus>, PaymentQueryArgs>({
                providesTags: ['Payments'],

                query: (queryArgs) => {
                    // REFACTOR: Removed paymentType parameter handling
                    // The new endpoint returns both incoming and outgoing payments by default,
                    // so we no longer append &paymentType=... to the URL.
                    // This simplifies the query string and avoids sending an unnecessary parameter.
                    const baseUrl = `/odata/paymentRecordsWithDueStatus?count=true&${toODataString(queryArgs)}`;

                    return {
                        url: baseUrl,
                        method: 'GET',
                    };
                },

                // REFACTOR: Updated transformResponse to work with the new DTO type
                // The response shape remains the same (OData array + count header),
                // but we type the returned data as PaymentWithDueStatus to reflect the new DueStatusArabic property.
                transformResponse: (response: any, meta, arg) => {
                    const { totalCount } = JSON.parse(meta!.response!.headers.get('count')!);
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            fetchPaymentsWithDueStatusByDateRange: builder.query<ListResponse<PaymentWithDueStatus>, { fromDate: string; toDate: string }>({
                query: ({ fromDate, toDate }) => ({
                    url: `/odata/paymentRecordsWithDueStatus/by-date-range?fromDate=${fromDate}&toDate=${toDate}`,
                    method: 'GET',
                }),
            }),
            addSalesOrderPayments: builder.mutation({
                invalidatesTags: ["Payments"],
                query: (payments) => {
                    return {
                        url: "/payments/createSalesOrderPayments",
                        method: "POST",
                        body: {...payments},
                    };
                },
            }),
            createPaymentAndFinAccountTrans: builder.mutation({
                invalidatesTags: ["Payments"],
                query: (payments) => {
                    return {
                        url: "/payments/createPaymentAndFinAccountTrans",
                        method: "POST",
                        body: {...payments},
                    };
                },
            }),
            duplicatePayment: builder.mutation<Payment, string>({
                query: (originalPaymentId) => ({
                    url: `/payments/duplicate/${originalPaymentId}`,
                    method: "POST",
                }),
                invalidatesTags: ["Payments"],
            }),
            updatePayment: builder.mutation({
                query: (payment) => {
                    return {
                        url: "/payments/updatePayment",
                        method: "PUT",
                        body: {...payment},
                    };
                },
                    invalidatesTags: ["Payments"]
            }),
            setPaymentStatus: builder.mutation({
                query: (changePaymentStatus) => {
                    return {
                        url: "/payments/setPaymentStatus",
                        method: "PUT",
                        body: {...changePaymentStatus},
                    };
                },
                invalidatesTags: ["Payments"]
            }),
            updateSalesOrderPayments: builder.mutation({
                query: (payments) => {
                    return {
                        url: "/payments/updateOrApproveSalesOrderPayments",
                        method: "PUT",
                        body: {...payments},
                    };
                },
            }),
            completeSalesOrderPayments: builder.mutation({
                query: (payments) => {
                    return {
                        url: "/payments/completeSalesOrderPayments",
                        method: "PUT",
                        body: {...payments},
                    };
                },
            }),
            fetchOutgoingPaymentTypes: builder.query<any[], any>({
                query: () => {
                    return {
                        url: `/payments/getPaymentsTypesOutgoing`,
                        method: "GET",
                    };
                },
            }),
            fetchIncomingPaymentTypes: builder.query<any[], any>({
                query: () => {
                    return {
                        url: `/payments/getPaymentsTypesIncoming`,
                        method: "GET",
                    };
                },
            }),
            fetchOutgoingPayments: builder.query<any[], any>({
                query: () => {
                    return {
                        url: `/payments/getPaymentsOutgoing`,
                        method: "GET",
                    };
                },
                transformResponse: (response: any) => {
                    return response.map((payment: any) => ({
                        ...payment,
                        text: `Payment: ${payment.paymentId} | Payment Type: ${payment.paymentTypeDescription} | Amount: ${payment.amount}`
                    }));
                }
            }),
            fetchIncomingPayments: builder.query<any[], any>({
                query: () => {
                    return {
                        url: `/payments/getPaymentsIncoming`,
                        method: "GET",
                    };
                },
                transformResponse: (response: any) => {
                    return response.map((payment: any) => ({
                        ...payment,
                        text: `${payment.paymentId} | ${payment.paymentTypeDescription} | ${payment.amount}`
                    }));
                }
            }),
            fetchAllPaymentTypes: builder.query<any[], any>({
                query: () => {
                    return {
                        url: `/payments/getPaymentsTypes`,
                        method: "GET",
                    };
                },
            }),
            fetchPaymentMethods: builder.query<any[], any>({
                query: () => {
                    return {
                        url: `/payments/getPaymentsMethods`,
                        method: "GET",
                    };
                },
            }),
            fetchPaymentMethodsByPartyId: builder.query<any, any>({
                query: (partyId) => {
                    return {
                        url: `/payments/${partyId}/getPaymentsMethodsByPartyId`,
                        method: "GET",
                    };
                },
            }),
            fetchPaymentApplicationsForPayment: builder.query<any[], any>({
                query: (paymentId) => {
                    return {
                        url: `/payments/${paymentId}/getPaymentsApplicationsForPayment`,
                        params: paymentId,
                        method: "GET",
                    };
                },
            }),
            createPaymentApplication: builder.mutation<
                PaymentApplicationParam,
                PaymentApplicationParam
                >({
                query: (body) => ({
                    url: "/payments/createPaymentApplication",
                    method: "POST",
                    body,
                }),
                invalidatesTags: (result, error, arg) => [
                    { type: 'PaymentApplications', id: arg.paymentId },
                    { type: 'NotAppliedInvoices', id: arg.paymentId },
                    "Payments"
                ],
            }),
            calculatePaymentTotals: builder.mutation<
                { id: string; amountToApply: number }[],
                string[]
                >({
                query: (paymentsIds) => ({
                    url: "/payments/CalculatePaymentTotals",
                    method: "POST",
                    body: paymentsIds, // Pass the array of invoice IDs
                }),
            }),
            removePaymentApplication: builder.mutation<RemovePaymentApplicationResponse, { paymentApplicationId: string; paymentId: string }>({
                query: ({ paymentApplicationId }) => ({
                    url: `paymentApplications/${paymentApplicationId}`,
                    method: 'DELETE',
                }),
                // REFACTOR: Invalidate cache tags
                // Technical: Invalidates related queries to refresh UI
                // Business Purpose: Ensures PaymentApplications, NotAppliedInvoices, and NotAppliedPayments reflect the deletion
                invalidatesTags: (result, error, { paymentId }) => [
                    { type: 'PaymentApplications', id: paymentId },
                    { type: 'NotAppliedInvoices', id: paymentId },
                    // 'NotAppliedPayments' retained if needed elsewhere; add providesTags to relevant query if not already
                ],
            }),
            fetchDailyPaymentsLazy: builder.query<
                { data: PaymentRecordDto[]; total: number },
                { paymentType: 'incoming' | 'outgoing' }
                >({
                query: ({ paymentType }) => ({
                    url: `/payments/daily`,
                    method: 'GET',
                    params: { paymentType },
                }),
                providesTags: ['DailyPayments'],
            }),
            getPaymentReportPdf: builder.query<ArrayBuffer, string>({
                query: (paymentId) => ({
                    url: `/paymentReports/payment-report/${paymentId}`,
                    responseHandler: (response) => response.arrayBuffer(), // Important: get raw bytes
                    cache: "no-cache",
                }),
            }),
            fetchPaymentsByDateRange: builder.query<
                { data: PaymentRecordDto[]; total: number },
                { paymentType: 'incoming' | 'outgoing'; fromDate: string; toDate: string }
                >({
                query: ({ paymentType, fromDate, toDate }) => ({
                    url: `/payments/by-date-range`,
                    method: 'GET',
                    params: { paymentType, fromDate, toDate },
                }),
                providesTags: ['PaymentsByDateRange'],
            }),
            fetchPaymentsForExport: builder.query<Payment[], { oDataQuery: string; paymentType: 'incoming' | 'outgoing' }>({
                query: ({ oDataQuery, paymentType }) => ({
                    url: `/odata/paymentRecords?${oDataQuery}&paymentType=${paymentType}`,
                    method: 'GET',
                }),
                transformResponse: (response: any) =>
                    Array.isArray(response) ? response : (response?.value ?? []),
            }),
            deletePayment: builder.mutation<void, string>({
                query: (paymentId) => ({
                    url: `/payments/${paymentId}`,
                    method: 'DELETE',
                }),
                // Invalidate the payments list so the grid refreshes automatically
                invalidatesTags: ['Payments'],
            }),
            resetPayment: builder.mutation<{ message: string }, string>({
                query: (paymentId) => ({
                    url: `/payments/reset/${paymentId}`,
                    method: "POST",
                }),
                invalidatesTags: (result, error, paymentId) => [
                    { type: "Payments", id: paymentId },
                    "Payments",           // list
                    "PaymentApplications",
                    "AccountingTransactions",
                ],
            }),
        };
    },
});

export const {
    useFetchPaymentsQuery,
    useFetchIncomingPaymentTypesQuery,
    useFetchOutgoingPaymentTypesQuery,
    useFetchIncomingPaymentsQuery,
    useFetchOutgoingPaymentsQuery,
    useFetchAllPaymentTypesQuery,
    useFetchPaymentMethodsByPartyIdQuery,
    useFetchPaymentMethodsQuery,
    useAddSalesOrderPaymentsMutation,
    useUpdateSalesOrderPaymentsMutation,
    useCompleteSalesOrderPaymentsMutation,
    useCreatePaymentAndFinAccountTransMutation,
    useUpdatePaymentMutation,
    useSetPaymentStatusMutation,
    useFetchPaymentApplicationsForPaymentQuery,
    useCalculatePaymentTotalsMutation,
    useRemovePaymentApplicationMutation,
    useCreatePaymentApplicationMutation,
    useLazyFetchDailyPaymentsLazyQuery,
    useFetchPaymentsWithDueStatusQuery,
    useLazyFetchPaymentsWithDueStatusByDateRangeQuery,
    useLazyGetPaymentReportPdfQuery,
    useLazyFetchPaymentsByDateRangeQuery,
    useDeletePaymentMutation,
    useDuplicatePaymentMutation, useResetPaymentMutation,
    useLazyFetchPaymentsForExportQuery,
} = paymentsApi;
export {paymentsApi};
