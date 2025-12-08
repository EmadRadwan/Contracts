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
    tagTypes: ["GlobalGl"],
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
                providesTags: ["GlobalGl"],
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
            }),
            fetchAdvancePaymentGlAccounts: builder.query<AdvancePaymentGlAccountDto[], void>({
                query: () => ({
                    url: '/glAccounts/getAdvancePaymentGlAccounts',
                }),
            }),
            createGlAccount: builder.mutation<CreateGlAccountResponse, CreateGlAccountRequest>({
                query: (request) => ({
                    url: '/glAccounts',
                    method: 'POST',
                    body: request,
                }),
                invalidatesTags: ['GlobalGl'],
            }),
        };
    },
});

export const {
    useFetchGlobalGlAccountsQuery,
    useFetchGlobalGlAccountSettingsQuery,
    useLazyFetchGlobalGlAccountSettingsQuery,
    useFetchTopLevelGlobalGlAccountsQuery,
    useFetchChildrenGlAccountsQuery,
    useFetchAdvancePaymentGlAccountsQuery,
    useCreateGlAccountMutation,
} = globalGlSettingsApi;
export {globalGlSettingsApi};

export interface AdvancePaymentGlAccountDto {
    glAccountId: string;
    accountCode: string;
    accountName: string;
    description: string;
    glAccountTypeId: string;
    glAccountClassId: string;
}

export interface CreateGlAccountRequest {
    accountName?: string;
    glResourceTypeId?: string;
    glAccountTypeId?: string;
    glAccountClassId?: string;
    parentGlAccountId?: string;
    description?: string;
}

export interface CreateGlAccountResponse {
    glAccountId?: string;
    accountCode?: string;
    accountName?: string;
    description?: string;
    glAccountTypeId?: string;
    glAccountClassId?: string;
    glResourceTypeId?: string;
    parentGlAccountId?: string;
    createdDate?: string;
}

