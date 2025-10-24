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
    tagTypes: ["MultiPaymentCertificates"],
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
                invalidatesTags: ['MultiPaymentCertificates'],
            }),
            approveMultiPaymentCertificate: builder.mutation<MultiPaymentCertificate, { workEffortId: string; companyId: string }>({
                query: ({ workEffortId, companyId }) => ({
                    // REFACTOR: Changed to a fixed endpoint URL and moved workEffortId to the request body,
                    // ensuring both workEffortId and companyId are sent together in the POST body for consistency
                    // and to simplify the backend route handling.
                    url: `/project/approveMultiPaymentCertificate`,
                    method: 'POST',
                    body: { workEffortId, companyId }
                }),
                invalidatesTags: ['MultiPaymentCertificate', 'MultiPaymentItems'],
            }),
        };
    },
});

export const { useFetchMultiPaymentCertificatesQuery,
    useGetMultiPaymentItemsQuery,
    useFetchSubProjectsQuery, useAddMultiPaymentCertificateMutation,
    useApproveMultiPaymentCertificateMutation, useUpdateMultiPaymentCertificateMutation
} = multiPaymentCertificateApi;
export { multiPaymentCertificateApi };


interface SubProject {
    workEffortId: string;
    subProjectName: string;
    projectId: string;
}