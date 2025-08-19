import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {State, toODataString} from "@progress/kendo-data-query";
import {AcctgTrans} from "../../../models/accounting/acctgTrans";
import {AcctgTransEntry} from "../../../models/accounting/acctgTransEntry";
import {setUiAcctgTransEntriesFromApi} from "../../../../features/accounting/slice/accountingSharedUiSlice";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const acctTransApi = createApi({
    reducerPath: "acctTrans",
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
    tagTypes: ["ITransactions", "PTransactions", "Transactions", "AcctgTransTypes"],
    endpoints(builder) {
        return {
            fetchAcctTrans: builder.query<ListResponse<AcctgTrans>, State>({
                query: (queryArgs) => {
                    const url = `/odata/accountingTransactionRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                transformResponse: (response: any, meta, arg) => {

                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            fetchAcctgTransTypes: builder.query<any[], any>({
                query: () => {
                    return {
                        url: `/accountingTransactionTypes/getAcctgTransTypesList`,
                        method: "GET",
                    };
                },
            }),
            createAcctgTransQuick: builder.mutation({
                query: (acctgTrans) => {
                    return {
                        url: "/transactions/quickCreateAcctgTrans",
                        method: "POST",
                        body: {...acctgTrans},
                    };
                },
            }),
            createAcctgTrans: builder.mutation({
                query: (acctgTrans) => {
                    return {
                        url: "/transactions/createAcctgTrans",
                        method: "POST",
                        body: {...acctgTrans},
                    };
                },
            }),
            createAcctgTransEntry: builder.mutation({
                query: (acctgTransEntry) => {
                    return {
                        url: "/transactions/createAcctgTransEntry",
                        method: "POST",
                        body: {...acctgTransEntry},
                    };
                },
                invalidatesTags: ["ITransactions"],
            }),
            updateAcctgTransEntry: builder.mutation({
                query: (acctgTransEntry) => {
                    return {
                        url: "/transactions/updateAcctgTransEntry",
                        method: "PUT",
                        body: {...acctgTransEntry},
                    };
                },
                invalidatesTags: ["ITransactions"],
            }),
            updateAcctgTrans: builder.mutation({
                query: (acctgTrans) => {
                    return {
                        url: "/transactions/updateAcctgTrans",
                        method: "PUT",
                        body: {...acctgTrans},
                    };
                },
            }),
            deleteAcctgTransEntry: builder.mutation({
                query: ({acctgTransId, acctgTransEntrySeqId}) => {
                    return {
                        url: `/transactions/deleteAcctgTransEntry?acctgTransId=${encodeURIComponent(acctgTransId)}&acctgTransEntrySeqId=${encodeURIComponent(acctgTransEntrySeqId)}`,
                        method: "DELETE",
                    };
                },
                // REFACTORED: Invalidate tag after deletion
                invalidatesTags: ["ITransactions"],
            }),
            fetchInvoiceAcctTransEntries: builder.query<AcctgTransEntry[], { invoiceId: string; acctgTransTypeId: string }>({
                query: ({ invoiceId, acctgTransTypeId }) => {
                    return {
                        url: `/transactions/${invoiceId}/${acctgTransTypeId}/getInvoiceTransactions`,
                        method: "GET",
                    };
                },
                providesTags: ["ITransactions"],
                transformResponse: (response: any, meta, arg) => {
                    return response as AcctgTransEntry[];
                },
            }),
            fetchPaymentAcctTransEntries: builder.query<AcctgTransEntry[], any>({
                query: (paymentId) => {
                    return {
                        url: `/transactions/${paymentId}/getPaymentTransactions`,
                        params: paymentId,
                        method: "GET",
                    };
                },
                providesTags: ["PTransactions"],
                transformResponse: (response: any, meta, arg) => {
                    return response as AcctgTransEntry[];
                },
            }),
            fetchGeneralAcctTransEntries: builder.query<AcctgTransEntry[], any>({
                query: (acctgTransId) => {
                    return {
                        url: `/transactions/${acctgTransId}/getGeneralTransactions`,
                        params: acctgTransId,
                        method: "GET",
                    };
                },
                providesTags: ["ITransactions"],
                async onQueryStarted(id, { dispatch, queryFulfilled }) {
                    try {
                        const { data } = await queryFulfilled;
                        console.log("Fetched entries for acctgTransId:", id, "Data:", data);
                        dispatch(setUiAcctgTransEntriesFromApi(data));
                    } catch (err) {
                        console.error("Error fetching entries:", err);
                    }
                },
                transformResponse: (response: any) => response as AcctgTransEntry[],
            }),
            fetchAcctTransEntries: builder.query<ListResponse<AcctgTransEntry>, State>({
                query: (queryArgs) => {
                    const url = `/odata/accountingTransactionEntryRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                providesTags: ["Transactions"],
                transformResponse: (response: any, meta, arg) => {

                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            postAcctgTrans: builder.mutation<string[], { acctgTransId: string; verifyOnly: boolean }>({
                query: ({ acctgTransId, verifyOnly }) => ({
                    url: `/transactions/postAcctgTrans`,
                    method: 'POST',
                    body: { acctgTransId, verifyOnly },
                }),
            }),
        };
    },
});

export const {
    useFetchAcctTransQuery, useFetchAcctgTransTypesQuery,
    useCreateAcctgTransQuickMutation, useCreateAcctgTransMutation,
    useUpdateAcctgTransMutation, useCreateAcctgTransEntryMutation,
    useUpdateAcctgTransEntryMutation, useDeleteAcctgTransEntryMutation,
    useFetchInvoiceAcctTransEntriesQuery,
    useFetchPaymentAcctTransEntriesQuery,
    useFetchAcctTransEntriesQuery,
    useFetchGeneralAcctTransEntriesQuery, usePostAcctgTransMutation
} = acctTransApi;
export {acctTransApi};
