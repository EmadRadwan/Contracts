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
            fetchGlReports: builder.query<GlReportsEnvelope, GlReportParams>({
                query: (params) => ({
                    url: '/glAccounts/getGlReports',
                    params,
                }),
            }),
            fetchGlClassCourses: builder.query<GlClassCoursesEnvelope, GlClassCourseParams>({
                query: (params) => ({
                    url: '/glAccounts/getGlClassCourses',
                    params,
                }),
            }),
            fetchGlSubClasses: builder.query<GlSubClassesEnvelope, GlSubClassParams>({
                query: (params) => ({
                    url: '/glAccounts/getGlSubClasses',
                    params,
                }),
            }),
            fetchGlSubClasses2: builder.query<GlSubClasses2Envelope, GlSubClass2Params>({
                query: (params) => ({
                    url: '/glAccounts/getGlSubClasses2',
                    params,
                }),
            }),
            fetchGlAccountCourseLabels: builder.query<GlAccountCourseLabelsEnvelope, GlAccountCourseLabelParams>({
                query: (params) => ({
                    url: '/glAccounts/getGlAccountCourseLabels',
                    params,
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
            updateGlAccount: builder.mutation<UpdateGlAccountResponse, UpdateGlAccountRequest>({
                query: ({glAccountId, ...body}) => ({
                    url: `/glAccounts/${glAccountId}`,
                    method: 'PUT',
                    body,
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
    useFetchGlReportsQuery,
    useFetchGlClassCoursesQuery,
    useFetchGlSubClassesQuery,
    useFetchGlSubClasses2Query,
    useFetchGlAccountCourseLabelsQuery,
    useCreateGlAccountMutation,
    useUpdateGlAccountMutation,
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
    glReportId?: string;
    glClassCourseId?: string;
    glSubClassId?: string;
    glSubClass2Id?: string;
    glAccountCourseLabelId?: string;
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
    glAccountTypeDescription?: string;
    glAccountClassDescription?: string;
    glReportId?: string;
    glClassCourseId?: string;
    glSubClassId?: string;
    glSubClass2Id?: string;
    glAccountCourseLabelId?: string;
}

export interface UpdateGlAccountRequest {
    glAccountId: string;
    accountName?: string;
    description?: string;
    parentGlAccountId?: string;
    glAccountTypeId?: string;
    glAccountClassId?: string;
    glResourceTypeId?: string;
    glReportId?: string;
    glClassCourseId?: string;
    glSubClassId?: string;
    glSubClass2Id?: string;
    glAccountCourseLabelId?: string;
}

export interface UpdateGlAccountResponse {
    glAccountId?: string;
    accountCode?: string;
    accountName?: string;
    description?: string;
    glAccountTypeId?: string;
    glAccountClassId?: string;
    glResourceTypeId?: string;
    parentGlAccountId?: string;
    lastUpdatedDate?: string;
    glAccountTypeDescription?: string;
    glAccountClassDescription?: string;
    glReportId?: string;
    glClassCourseId?: string;
    glSubClassId?: string;
    glSubClass2Id?: string;
    glAccountCourseLabelId?: string;
}

export interface GlReportDto {
    glReportId: string;
    description: string;
}

export interface GlReportsEnvelope {
    glReports: GlReportDto[];
    totalCount: number;
}

export interface GlReportParams {
    skip?: number;
    pageSize?: number;
    searchTerm?: string;
}

export interface GlClassCourseDto {
    glClassCourseId: string;
    description: string;
}

export interface GlClassCoursesEnvelope {
    glClassCourses: GlClassCourseDto[];
    totalCount: number;
}

export interface GlClassCourseParams {
    skip?: number;
    pageSize?: number;
    searchTerm?: string;
}

export interface GlSubClassDto {
    glSubClassId: string;
    description: string;
}

export interface GlSubClassesEnvelope {
    glSubClasses: GlSubClassDto[];
    totalCount: number;
}

export interface GlSubClassParams {
    skip?: number;
    pageSize?: number;
    searchTerm?: string;
}

export interface GlSubClass2Dto {
    glSubClass2Id: string;
    description: string;
}

export interface GlSubClasses2Envelope {
    glSubClasses2: GlSubClass2Dto[];
    totalCount: number;
}

export interface GlSubClass2Params {
    skip?: number;
    pageSize?: number;
    searchTerm?: string;
}

export interface GlAccountCourseLabelDto {
    glAccountCourseLabelId: string;
    description: string;
}

export interface GlAccountCourseLabelsEnvelope {
    glAccountCourseLabels: GlAccountCourseLabelDto[];
    totalCount: number;
}

export interface GlAccountCourseLabelParams {
    skip?: number;
    pageSize?: number;
    searchTerm?: string;
}

