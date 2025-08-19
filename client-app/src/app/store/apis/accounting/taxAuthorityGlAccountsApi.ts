import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../../configureStore";
import { State } from "@progress/kendo-data-query";

const taxAuthoritiesGlAccountsApi = createApi({
  reducerPath: "taxAuthoritiesGlAccounts",
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
      fetchTaxAuthoritiesGlAccounts: builder.query<any[], any>({
        query: (companyId) => {
          return {
            url: `/organizationGl/${companyId}/getTaxAuthorityGlAccounts`,
            params: companyId,
            method: "GET",
          };
        },
      }),
    };
  },
});

export const { useFetchTaxAuthoritiesGlAccountsQuery } =
  taxAuthoritiesGlAccountsApi;
export { taxAuthoritiesGlAccountsApi };
