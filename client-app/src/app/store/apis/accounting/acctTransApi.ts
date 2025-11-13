import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {State, toODataString} from "@progress/kendo-data-query";
import {AcctgTrans} from "../../../models/accounting/acctgTrans";
import {AcctgTransEntry} from "../../../models/accounting/acctgTransEntry";
import {setUiAcctgTransEntriesFromApi} from "../../../../features/accounting/slice/accountingSharedUiSlice";
import {
    UpdateMultiAcctgTransResponse,
    UpdateMultiAcctgTransWithEntriesParams
} from "../../../../features/accounting/transaction/hook/useEditMultiAcctgTrans";
import {
    CreateInitialBalanceTransParams,
    CreateInitialBalanceTransResponse
} from "../../../../features/accounting/transaction/hook/useInitialBalanceTrans";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const acctTransApi = createApi({
    reducerPath: "acctTrans",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers) => {
            const token = store.getState().account.user?.token;
            const lang = store.getState().localization.language;

            if (token) headers.set("authorization", `Bearer ${token}`);
            if (lang) headers.set("Accept-Language", lang);
            return headers;
        },
    }),
    tagTypes: [
        "ITransactions",
        "PTransactions",
        "Transactions",
        "AcctgTransTypes",
        "MultiTransactions",
        "GlAccounts"
    ],
    endpoints(builder) {
        return {
            fetchAcctTrans: builder.query<ListResponse<AcctgTrans>, State>({
                query: (queryArgs) => {
                    const odataQuery = toODataString({
                        ...queryArgs,
                        filter: queryArgs.filter || {logic: 'and', filters: []},
                    });
                    return {
                        url: `/odata/accountingTransactionRecords?$count=true&${odataQuery}&companyId=${encodeURIComponent(queryArgs.companyId)}`,
                        method: 'GET',
                    };
                },
                providesTags: ["Transactions"],
                transformResponse: (response: any, meta) => {
                    const {totalCount} = JSON.parse(meta!.response!.headers.get("count")!);
                    return {data: response, total: totalCount};
                },
            }),

            fetchAcctgTransTypes: builder.query<any[], void>({
                query: () => ({
                    url: `/accountingTransactionTypes/getAcctgTransTypesList`,
                    method: "GET",
                }),
                providesTags: ["AcctgTransTypes"],
            }),

            createAcctgTransQuick: builder.mutation({
                query: (acctgTrans) => ({
                    url: "/transactions/quickCreateAcctgTrans",
                    method: "POST",
                    body: acctgTrans,
                }),
                invalidatesTags: ["Transactions"],
            }),

            createAcctgTrans: builder.mutation({
                query: (acctgTrans) => ({
                    url: "/transactions/createAcctgTrans",
                    method: "POST",
                    body: acctgTrans,
                }),
                invalidatesTags: ["Transactions"],
            }),

            createAcctgTransEntry: builder.mutation({
                query: (acctgTransEntry) => ({
                    url: "/transactions/createAcctgTransEntry",
                    method: "POST",
                    body: acctgTransEntry,
                }),
                invalidatesTags: ["ITransactions"],
            }),

            updateAcctgTransEntry: builder.mutation({
                query: (acctgTransEntry) => ({
                    url: "/transactions/updateAcctgTransEntry",
                    method: "PUT",
                    body: acctgTransEntry,
                }),
                invalidatesTags: ["ITransactions"],
            }),

            updateAcctgTrans: builder.mutation({
                query: (acctgTrans) => ({
                    url: "/transactions/updateAcctgTrans",
                    method: "PUT",
                    body: acctgTrans,
                }),
                invalidatesTags: ["Transactions"],
            }),

            deleteAcctgTransEntry: builder.mutation({
                query: ({acctgTransId, acctgTransEntrySeqId}) => ({
                    url: `/transactions/deleteAcctgTransEntry?acctgTransId=${encodeURIComponent(acctgTransId)}&acctgTransEntrySeqId=${encodeURIComponent(acctgTransEntrySeqId)}`,
                    method: "DELETE",
                }),
                invalidatesTags: ["ITransactions"],
            }),

            fetchInvoiceAcctTransEntries: builder.query<AcctgTransEntry[], { invoiceId: string; acctgTransTypeId: string }>({
                query: ({invoiceId, acctgTransTypeId}) => ({
                    url: `/transactions/${invoiceId}/${acctgTransTypeId}/getInvoiceTransactions`,
                    method: "GET",
                }),
                providesTags: ["ITransactions"],
            }),

            fetchPaymentAcctTransEntries: builder.query<AcctgTransEntry[], string>({
                query: (paymentId) => ({
                    url: `/transactions/${paymentId}/getPaymentTransactions`,
                    method: "GET",
                }),
                providesTags: ["PTransactions"],
            }),

            fetchGeneralAcctTransEntries: builder.query<AcctgTransEntry[], string>({
                query: (acctgTransId) => ({
                    url: `/transactions/${acctgTransId}/getGeneralTransactions`,
                    method: "GET",
                }),
                providesTags: ["ITransactions"],
                async onQueryStarted(id, {dispatch, queryFulfilled}) {
                    try {
                        const {data} = await queryFulfilled;
                        dispatch(setUiAcctgTransEntriesFromApi(data));
                    } catch {}
                }
            }),

            fetchAcctTransEntries: builder.query<ListResponse<AcctgTransEntry>, State>({
                query: (queryArgs) => {
                    const odataQuery = toODataString({
                        ...queryArgs,
                        filter: queryArgs.filter || {logic: 'and', filters: []},
                    });
                    return {
                        url: `/odata/accountingTransactionEntryRecords?$count=true&${odataQuery}&companyId=${encodeURIComponent(queryArgs.companyId)}`,
                        method: 'GET',
                    };
                },
                providesTags: ["ITransactions"],
                transformResponse: (response: any, meta) => {
                    const {totalCount} = JSON.parse(meta!.response!.headers.get("count")!);
                    return {data: response, total: totalCount};
                },
            }),

            postAcctgTrans: builder.mutation<string[], { acctgTransId: string; verifyOnly: boolean }>({
                query: ({acctgTransId, verifyOnly}) => ({
                    url: `/transactions/postAcctgTrans`,
                    method: 'POST',
                    body: {acctgTransId, verifyOnly},
                }),
                invalidatesTags: ["Transactions", "ITransactions", "PTransactions"],
            }),

            createMultiAcctgTransWithEntries: builder.mutation({
                query: (params) => ({
                    url: "/transactions/createMultiAcctgTransWithEntries",
                    method: "POST",
                    body: params,
                }),
                invalidatesTags: ["MultiTransactions", "Transactions"],
            }),

            updateMultiAcctgTransWithEntries: builder.mutation<UpdateMultiAcctgTransResponse, UpdateMultiAcctgTransWithEntriesParams>({
                query: (params) => ({
                    url: '/transactions/updateMultiAcctgTransWithEntries',
                    method: 'PUT',
                    body: params,
                }),
                invalidatesTags: ["MultiTransactions", "Transactions", "ITransactions"],
            }),

            createInitialBalanceTrans: builder.mutation<
                CreateInitialBalanceTransResponse,
                CreateInitialBalanceTransParams
                >({
                query: (params) => ({
                    url: "/transactions/createInitialBalanceTrans",
                    method: "POST",
                    body: params,
                }),
                invalidatesTags: ["GlAccounts", "Transactions"],
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
    useFetchGeneralAcctTransEntriesQuery,
    usePostAcctgTransMutation,
    useCreateMultiAcctgTransWithEntriesMutation,
    useUpdateMultiAcctgTransWithEntriesMutation,
    useCreateInitialBalanceTransMutation
} = acctTransApi;

export {acctTransApi};
