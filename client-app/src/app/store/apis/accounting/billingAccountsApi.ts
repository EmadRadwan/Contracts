import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {State, toODataString} from "@progress/kendo-data-query";
import {BillingAccount} from "../../../models/accounting/billingAccount";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const billingAccountsApi = createApi({
    reducerPath: "billingAccounts",
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
            fetchBillingAccounts: builder.query<ListResponse<BillingAccount>, State>({
                query: (queryArgs) => {
                    const url = `/odata/billingAccountRecords?$count=true&${toODataString(queryArgs)}`;
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
            fetchBillingAccountsBalance: builder.query<any, any>({
                query: (billingAccountId) => {
                    return {
                        url: `/billingAccounts/${billingAccountId}/getBillingAccountBalance`,
                        method: "GET",
                    };
                }
            }),
            fetchBillingAccountsInvoices: builder.query<any, {billingAccountId: string, statusId: string | null}>({
                query: ({billingAccountId, statusId}) => {
                    return {
                        url: `/billingAccounts/${billingAccountId}/listBillingAccountInvoices/${statusId}`,
                        method: "GET",
                    };
                }
            }),
            fetchBillingAccountsPayments: builder.query<any, {billingAccountId: string}>({
                query: ({billingAccountId}) => {
                    return {
                        url: `/billingAccounts/${billingAccountId}/getBillingAccountPayments`,
                        method: "GET",
                    };
                }
            }),
            fetchBillingAccountsOrders: builder.query<any, {billingAccountId: string}>({
                query: ({billingAccountId}) => {
                    return {
                        url: `/billingAccounts/${billingAccountId}/getBillingAccountOrders`,
                        method: "GET",
                    };
                }
            }),
            createBillingAccountPayment: builder.mutation({
                query: (body) => {
                    return {
                        url: "/payments/createPaymentAndApplicationForBillingAccount",
                        method: "POST",
                        body: {...body}
                    }
                }
            }),
            fetchBillingAccountsForParty: builder.query({
                query: (partyId: string) => `/billingAccounts/getBillingAccountsForParty?partyId=${partyId}`,
            }),
        };
    },
});

export const {
    useFetchBillingAccountsQuery,
    useFetchBillingAccountsBalanceQuery,
    useFetchBillingAccountsInvoicesQuery,
    useFetchBillingAccountsPaymentsQuery,
    useFetchBillingAccountsOrdersQuery,
    useCreateBillingAccountPaymentMutation, useFetchBillingAccountsForPartyQuery
} = billingAccountsApi;
export {billingAccountsApi};
