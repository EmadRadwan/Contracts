import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {GlAccount} from "../../../models/accounting/globalGlSettings";

const orgChartOfAccountsLovApi = createApi({
    reducerPath: "orgChartOfAccountsLov",
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
    tagTypes: ["OrgChartOfAccountsLov"],
    endpoints(builder) {
        return {
            fetchOrgChartOfAccountsLov: builder.query<GlAccount[], any>({
                query: (undefined) => {
                    // console.log("queryArgs order item", orderId)
                    return {
                        url: `/organizationGl/getGlAccountHierarchyLov`,
                        params: undefined,
                        method: "GET",
                    };
                }
            }),
            fetchGlAccountOrganizationHierarchyLov: builder.query<any, string>({
                query: (companyId) => ({
                    url: `/organizationGl/${companyId}/getGlAccountOrganizationHierarchyLov`,
                    method: "GET",
                }),
                providesTags: ["OrgChartOfAccountsLov"],
            }),
            fetchGlAccountOrgCashOrEquivalentLov: builder.query<any, string>({
                query: (companyId) => ({
                    url: `/organizationGl/${companyId}/getGlAccountOrgCashOrEquivalentLov`,
                    method: "GET",
                }),
            }),
            fetchGlAccountOrganizationGlAccounts: builder.query<any[], any>({
                query: (companyId) => {
                    return {
                        url: `/organizationGl/${companyId}/getOrganizationGlAccounts`,
                        method: "GET",
                    };
                },

            }),
        };
    },
});

export const {
    useFetchOrgChartOfAccountsLovQuery,
    useFetchGlAccountOrganizationGlAccountsQuery,
    useFetchGlAccountOrganizationHierarchyLovQuery,
    useFetchGlAccountOrgCashOrEquivalentLovQuery,
} = orgChartOfAccountsLovApi;
export {orgChartOfAccountsLovApi};
