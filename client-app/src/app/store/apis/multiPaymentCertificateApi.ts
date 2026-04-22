import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../configureStore";
import { State, toODataString } from "@progress/kendo-data-query";
import { MultiPaymentCertificate } from "../../../app/models/project/MultiPaymentCertificate";
import {MultiPaymentItem} from "../../models/project/MultiPaymentItem";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const multiPaymentCertificateApi = createApi({
    reducerPath: "multiPaymentCertificates",
    tagTypes: ["MultiPaymentCertificates", "MultiPaymentItems"],
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, { getState }) => {
            // REFACTOR: Reuse authentication and localization logic from projectsApi for consistency
            const token = store.getState().account.user?.token;
            const lang = store.getState().localization.language;

            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            if (lang) {
                headers.set("Accept-Language", lang);
            }
            return headers;
        },
    }),
    endpoints(builder) {
        return {
            fetchMultiPaymentCertificates: builder.query<ListResponse<MultiPaymentCertificate>, State>({
                query: (queryArgs) => {
                    const url = `/odata/MultiPaymentCertificateRecords?$count=true&${toODataString(queryArgs)}`;
                    return { url, method: "GET" };
                },
                transformResponse: (response: any, meta, arg) => {
                    // REFACTOR: Extract total count from headers and format response, consistent with projectsApi
                    const { totalCount } = JSON.parse(meta!.response!.headers.get("count")!);
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
                providesTags: ["MultiPaymentCertificates"],
            }),
            getMultiPaymentItems: builder.query<MultiPaymentItem[], string>({
                query: (certificateId) => `project/${certificateId}/items`,
                providesTags: ["MultiPaymentItems"],
            }),
            fetchSubProjects: builder.query<ListResponse<SubProject>, string>({
                query: (projectId) => `project/subProjects/${projectId}`,
                providesTags: ["SubProjects"],
            }),
            addMultiPaymentCertificate: builder.mutation<MultiPaymentCertificate, MultiPaymentCertificate>({
                query: (certificate) => ({
                    url: '/project/multiPaymentCertificate',
                    method: 'POST',
                    body: certificate,
                }),
                invalidatesTags: ['MultiPaymentCertificates'],
            }),
            updateMultiPaymentCertificate: builder.mutation<MultiPaymentCertificate, MultiPaymentCertificate>({
                query: (certificate) => ({
                    url: `/project/multiPaymentCertificate/${certificate.workEffortId}`,
                    method: 'PUT',
                    body: certificate,
                }),
                invalidatesTags: ['MultiPaymentCertificates', 'MultiPaymentItems'],
            }),
            approveMultiPaymentCertificate: builder.mutation<MultiPaymentCertificate, { workEffortId: string; companyId: string }>({
                query: ({ workEffortId, companyId }) => ({
                    url: `/project/approveMultiPaymentCertificate`,
                    method: 'POST',
                    body: { workEffortId, companyId }
                }),
                invalidatesTags: ['MultiPaymentCertificates', 'MultiPaymentItems'],
            }),
            deleteMultiPaymentCertificate: builder.mutation<void, string>({
                query: (workEffortId) => ({
                    url: `/project/multiPaymentCertificate/${workEffortId}`,
                    method: 'DELETE',
                }),
                invalidatesTags: ['MultiPaymentCertificates'],
            }),
            resetMultiPaymentCertificate: builder.mutation<MultiPaymentCertificate, string>({
                query: (workEffortId) => ({
                    url: `/project/resetMultiPaymentCertificate/${workEffortId}`,
                    method: 'POST',
                }),
                invalidatesTags: ['MultiPaymentCertificates', 'MultiPaymentItems'],
            }),
            duplicateMultiPaymentCertificate: builder.mutation<MultiPaymentCertificate, string>({
                query: (workEffortId) => ({
                    url: `/project/${workEffortId}/duplicateMultiPaymentCertificate`,
                    method: 'POST',
                }),
                invalidatesTags: ['MultiPaymentCertificates'],
            }),
            fetchMultiPaymentCertificatesByDateRange: builder.query<MultiPaymentCertificate[], { startDate: string; endDate: string }>({
                query: ({ startDate, endDate }) => `/project/multiPaymentCertificates/by-date-range?startDate=${startDate}&endDate=${endDate}`,
                providesTags: ["MultiPaymentCertificates"],
            }),
            fetchMultiPaymentItemsByDateRange: builder.query<any[], { startDate: string; endDate: string }>({
                query: ({ startDate, endDate }) => `/project/multiPaymentItems/by-date-range?startDate=${startDate}&endDate=${endDate}`,
                providesTags: ["MultiPaymentItems"],
            }),
        };
    },
});

export const { useFetchMultiPaymentCertificatesQuery,
    useGetMultiPaymentItemsQuery,
    useFetchSubProjectsQuery, useAddMultiPaymentCertificateMutation,
    useApproveMultiPaymentCertificateMutation, useUpdateMultiPaymentCertificateMutation,
    useDeleteMultiPaymentCertificateMutation, useResetMultiPaymentCertificateMutation,
    useDuplicateMultiPaymentCertificateMutation,
    useLazyFetchMultiPaymentCertificatesByDateRangeQuery,
    useLazyFetchMultiPaymentItemsByDateRangeQuery
} = multiPaymentCertificateApi;
export { multiPaymentCertificateApi };


interface SubProject {
    workEffortId: string;
    subProjectName: string;
    projectId: string;
}