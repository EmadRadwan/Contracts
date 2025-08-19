import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {GlAccount} from "../../../models/accounting/globalGlSettings";
import {State, toODataString} from "@progress/kendo-data-query";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const globalGlSettingsApi = createApi({
    reducerPath: "globalGlSettings",
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

    endpoints(builder) {
        return {
            fetchGlobalGlAccounts: builder.query<ListResponse<GlAccount>, State>({
                query: (queryArgs) => {
                    const url = `/odata/GlobalGlAccountRecords?$count=true&${toODataString(queryArgs)}`;
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
            fetchTopLevelGlobalGlAccounts: builder.query<any, any>({
                query: () => {
                    const url = `/glAccounts/getGlAccountsLov`;
                    return {url, method: "GET"};
                },
            }),
            fetchChildrenGlAccounts: builder.query<any, string>({
                query: (parentGlAccountId) => {
                    const url = `/glAccounts/${parentGlAccountId}/getChildGlAccounts`;
                    return {url, method: "GET"};
                },
            }),            
            fetchGlobalGlAccountSettings: builder.query<GlAccount[], undefined>({
                query: () => {
                    return {
                        url: "/globalGlSettings",
                        method: "GET",
                    };
                },
                // transformResponse: (response: any, meta) => {
                //     let classesSet = [...new Set(response.map((res: any) => res.glAccountClassId))]
                //     let typesSet = [...new Set(response.map((res: any) => res.glAccountTypeId))].filter(t => t)
                //     let resourcesSet = [...new Set(response.map((res: any) => res.glResourceTypeId))]
                //     let parentAccounts = [...new Set(response.filter((res: any) => !res.parentGlAccountId).map((p: any) => {
                //         return {
                //             glParentAccountId: p.glAccountId,
                //             glParentAccountName: p.accountName
                //         }
                //     }))]
                //     // console.log(parentAccounts)
                //     let accountClasses = classesSet.map((c: any) => {
                //         return {
                //             glAccountClassId: c,
                //             glAccountClassDescription: c.split("_").map((g: string) => g[0].concat(g.substring(1).toLowerCase())).join(" ")
                //         }
                //     })
                //     let accountTypes = typesSet.map((t: any) => {
                //         return {
                //             glAccountTypeId: t,
                //             glAccountTypeDescription: t.split("_").map((g: string) => g[0].concat(g.substring(1).toLowerCase())).join(" ")
                //         }
                //     })
                //     let resourceTypes = resourcesSet.map((r: any) => {
                //         return {
                //             glResourceTypeId: r,
                //             glResourceTypeDescription: r.split("_").map((g: string) => g[0].concat(g.substring(1).toLowerCase())).join(" ")
                //         }
                //     })
                //     return {
                //         "accountClasses": accountClasses,
                //         "accountTypes": accountTypes,
                //         "resourceTypes": resourceTypes,
                //         "parentAccounts": parentAccounts
                //     }

                // },
            }),
        };
    },
});

export const {
    useFetchGlobalGlAccountsQuery,
    useFetchGlobalGlAccountSettingsQuery,
    useLazyFetchGlobalGlAccountSettingsQuery,
    useFetchTopLevelGlobalGlAccountsQuery,
    useFetchChildrenGlAccountsQuery
} = globalGlSettingsApi;
export {globalGlSettingsApi};
