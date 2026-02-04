import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../../configureStore";

const partyGlAccountsApi = createApi({
    reducerPath: "partyGlAccounts",
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
    tagTypes: ["PartyGlAccounts"],
    endpoints (builder) {
        return {
            fetchPartyGlAccounts: builder.query<any[], any>({
                query: (companyId) => {
                    return {
                        url: `/organizationGl/${companyId}/getPartyGlAccounts`,
                        params: companyId,
                        method: "GET"
                    }
                },
                providesTags: ["PartyGlAccounts"],
            }),
            fetchRoles: builder.query<any[], any>({
                query: () => {
                    return {
                        url: '/parties/listRoles',
                        method: "GET"
                    }
                }
            }),
            createPartyGlAccount: builder.mutation<
                { success: boolean; message?: string; partyGlAccountId?: string },
                CreatePartyGlAccountRequest
                >({
                query: (body) => ({
                    url: "/organizationGl/partyGlAccounts",     // ← choose your actual path
                    method: "POST",
                    body,
                }),
                invalidatesTags: ["PartyGlAccounts"],        // ← will refetch the list
            }),
        }
    }
})

export const {useFetchPartyGlAccountsQuery, useFetchRolesQuery, useCreatePartyGlAccountMutation
} = partyGlAccountsApi
export {partyGlAccountsApi}

export interface CreatePartyGlAccountRequest {
    companyId: string;              // or organizationPartyId
    partyId: string;
    roleTypeId: string;
    glAccountId: string;
    glAccountTypeId: string;
}