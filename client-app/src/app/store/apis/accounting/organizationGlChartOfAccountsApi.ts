import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {GlAccount} from "../../../models/accounting/globalGlSettings";
import {State, toODataString} from "@progress/kendo-data-query";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const organizationGlChartOfAccountsApi = createApi({
    reducerPath: "organizationGlChartOfAccounts",
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
            fetchOrganizationGlChartOfAccounts: builder.query<ListResponse<GlAccount>, { companyId?: string; dataState: State }>({

                query: ({companyId, dataState}) => {
                    const url = `/odata/organizationGlChartOfAccountRecords?$count=true&${toODataString(dataState)}&companyId=${companyId}`;
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
            fetchOrganizationGlAccountsByClass: builder.query<GlAccount[], {companyId: string, accountClass: string}>({
                query: ({companyId, accountClass}) => {
                    return {
                        url: `/organizationGl/${companyId}/getOrganizationGlAccountsByClass`,
                        params: {Class: accountClass},
                        method: "GET"
                    }
                },
                // transformResponse: (response: any[], meta, arg) => {
                //     const className = arg.accountClass.split("_").map((p: string) => p.toLowerCase()).map((p:string, i: number) => {
                //         if (i !== 0) {
                //             return p[0].toUpperCase().concat(p.substring(1))
                //         }
                //     }).join("")
                //     return response.map((account: GlAccount) => {
                //         return {
                //         }
                //     })
                // }
            }),
            fetchOrganizationGlAccountsByType: builder.query<GlAccount[], {companyId: string, accountType: string}>({
                query: ({companyId, accountType}) => {
                    return {
                        url: `/organizationGl/${companyId}/getOrganizationGlAccountsByAccountType`,
                        params: {Type: accountType},
                        method: "GET"
                    }
                }
            }),
            fetchFullChartOfAccounts: builder.query<GlAccount[], { companyId: string }>({
                query: ({ companyId }) => ({
                    url: `/OrganizationGl/chart-of-accounts`,
                    params: { companyId },
                    method: 'GET'
                })
            })
        };
    },
});

export const {
    useFetchOrganizationGlChartOfAccountsQuery,
    useFetchOrganizationGlAccountsByClassQuery,
    useFetchOrganizationGlAccountsByTypeQuery, useFetchFullChartOfAccountsQuery
} = organizationGlChartOfAccountsApi;
export {organizationGlChartOfAccountsApi};
