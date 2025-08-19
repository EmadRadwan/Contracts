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
    tagTypes: ["Payments"],
    endpoints(builder) {
        return {
            fetchPayments: builder.query<ListResponse<Payment>, State>({
                providesTags: ["Payments"],
                query: (queryArgs) => {
                    const url = `/odata/paymentRecords?count=true&${toODataString(queryArgs)}`
                    return {
                        url,
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta, arg) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                }
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
            updatePayment: builder.mutation({
                query: (payment) => {
                    return {
                        url: "/payments/updatePayment",
                        method: "PUT",
                        body: {...payment},
                    };
                },
            }),
            setPaymentStatusToReceived: builder.mutation({
                query: (changePaymentStatus) => {
                    return {
                        url: "/payments/setPaymentStatusToReceived",
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
            removePaymentApplication: builder.mutation<RemovePaymentApplicationResponse, { paymentApplicationId: string }>({
            query: ({ paymentApplicationId }) => ({
                url: `paymentApplications/${paymentApplicationId}`,
                method: 'DELETE',
            }),
            // REFACTOR: Invalidate cache tags
            // Technical: Invalidates related queries to refresh UI
            // Business Purpose: Ensures PaymentApplications, NotAppliedInvoices, and NotAppliedPayments reflect the deletion
            invalidatesTags: ['PaymentApplications', 'NotAppliedInvoices', 'NotAppliedPayments'],
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
    useSetPaymentStatusToReceivedMutation,
    useFetchPaymentApplicationsForPaymentQuery,
    useCalculatePaymentTotalsMutation, useRemovePaymentApplicationMutation
} = paymentsApi;
export {paymentsApi};
