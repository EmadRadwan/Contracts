import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../../configureStore";
import { State, toODataString } from "@progress/kendo-data-query";
import { FinancialAccount } from "../../../models/accounting/financialAccount";

interface ListResponse<T> {
  data: T[];
  total: number;
}

const financialAccountsApi = createApi({
  reducerPath: "financialAccounts",
  baseQuery: fetchBaseQuery({
    baseUrl: import.meta.env.VITE_API_URL,
    prepareHeaders: (headers, { getState }) => {
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
      fetchFinancialAccounts: builder.query<
        ListResponse<FinancialAccount>,
        State
      >({
        query: (queryArgs) => {
          const url = `/odata/finAccountRecords?$count=true&${toODataString(
            queryArgs
          )}`;
          return { url, method: "GET" };
        },
        transformResponse: (response: any, meta, arg) => {
          const { totalCount } = JSON.parse(
            meta!.response!.headers.get("count")!
          );
          return {
            data: response,
            total: totalCount,
          };
        },
      }),
      fetchFinAccountTransAndTotals: builder.query<any, any>({
        query: (queryArgs) => {
          return {
            url: "/finAccount/getTransListAndTotals",
            method: "POST",
            body: { ...queryArgs },
          };
        },
      }),
      fetchFinAccountTransTypes: builder.query({
        query: () => {
          return {
            url: "/finAccount/listFinAccountTransTypes",
            method: "GET",
          };
        },
      }),
      fetchFinAccountTransStatuses: builder.query({
        query: () => {
          return {
            url: "/finAccount/listFinAccountTransStatus",
            method: "GET",
          };
        },
      }),
      fetchFinAccountWidthrawDeposits: builder.query<any, any>({
        query: ({
                  checkFinAccountTransNull,
                  fromDate,
                  thruDate,
                  partyIdFrom,
                  partyIdSetFromFinAccountRole,
                  paymentMethodTypeId,
                }) => {
          return {
            url: "/finAccount/listFinAccountDepositsWidthrawls",
            method: "GET",
            params: {
              checkFinAccountTransNull,
              fromDate,
              thruDate,
              partyIdFrom,
              partyIdSetFromFinAccountRole,
              paymentMethodTypeId,
            },
          };
        },
      }),

      createFinAccountTransaction: builder.mutation({
        query: (body) => {
          return {
            url: "/finAccount/createFinAccountTrans",
            method: "POST",
            body: {...body}
          }
        }
      }),
      fetchFinAccountsForParty: builder.query({
        query: (partyId: string) => `/finAccount/getFinAccountsForParty?partyId=${partyId}`,
      }),
    };
  },
});

export const {
  useFetchFinancialAccountsQuery,
  useFetchFinAccountTransAndTotalsQuery,
  useLazyFetchFinAccountTransAndTotalsQuery,
  useFetchFinAccountTransTypesQuery,
  useFetchFinAccountTransStatusesQuery,
  useFetchFinAccountWidthrawDepositsQuery,
  useCreateFinAccountTransactionMutation, useFetchFinAccountsForPartyQuery,
} = financialAccountsApi;
export { financialAccountsApi };
