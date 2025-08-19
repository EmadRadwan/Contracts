import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../../configureStore";

const finAccountGlAccountsApi = createApi({
    reducerPath: "finAccountGlAccounts",
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

    endpoints (builder) {
        return {
            fetchFinAccountGlAccounts: builder.query<any[], any>({
                query: (companyId) => {
                    return {
                        url: `/organizationGl/${companyId}/getFinAccountTypeGlAccounts`,
                        params: companyId,
                        method: "GET"
                    }
                }
            })
        }
    }
})

export const {useFetchFinAccountGlAccountsQuery } = finAccountGlAccountsApi
export {finAccountGlAccountsApi}