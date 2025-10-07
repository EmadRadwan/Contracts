import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {Party} from "../../models/party/party";
import {State, toODataString} from "@progress/kendo-data-query";
import {WorkEffort} from "../../models/manufacturing/workEffort";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const projectsApi = createApi({
    reducerPath: "projects",
    tagTypes: ["WorkEffort", "ProjectCertificates"],
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
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
            fetchProjects: builder.query<ListResponse<WorkEffort>, State>({
                query: (queryArgs) => {
                    const url = `/odata/projectRecords?$count=true&${toODataString(queryArgs)}`;
                    return { url, method: "GET" };
                },
                transformResponse: (response: any, meta, arg) => {
                    const {totalCount} = JSON.parse(meta!.response!.headers.get("count")!);
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
                providesTags: ["WorkEffort"],
            }),
            addProject: builder.mutation<WorkEffort, Partial<WorkEffort>>({
                query: (project) => ({
                    url: "/project/createProject",
                    method: "POST",
                    body: { ...project },
                }),
                invalidatesTags: ["WorkEffort"],
            }),
            updateProject: builder.mutation<WorkEffort, Partial<WorkEffort>>({
                query: (project) => ({
                    url: `/project/${project.WorkEffortId}`,
                    method: "PUT",
                    body: project,
                }),
                invalidatesTags: ["WorkEffort"],
            }),
            fetchProjectCertificates: builder.query<ListResponse<WorkEffort>, State>({
                query: (queryArgs) => {
                    const url = `/odata/ProjectCertificateRecords?$count=true&${toODataString(queryArgs)}`;
                    return {
                        url,
                        method: "GET",
                    };
                },
                providesTags: ["ProjectCertificates"],
                transformResponse: (response: any, meta, arg) => {
                    const { totalCount } = JSON.parse(meta!.response!.headers.get("count")!);
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            addProjectCertificate: builder.mutation<ProjectCertificateRecord, Partial<ProjectCertificateRecord>>({
                query: (certificate) => ({
                    url: "/project/createProjectCertificate",
                    method: "POST",
                    body: { ...certificate },
                }),
                invalidatesTags: ["ProjectCertificates"],
            }),
            updateProjectCertificate: builder.mutation<ProjectCertificateRecord, Partial<ProjectCertificateRecord>>({
                query: (certificate) => {
                    if (!certificate.WorkEffortId) {
                        throw new Error("WorkEffortId is required for updating certificate");
                    }
                    return {
                        url: `/project/updateProjectCertificate/${certificate.WorkEffortId}`,
                        method: "PUT",
                        body: { ...certificate },   // ✅ send DTO directly
                    };
                },
                invalidatesTags: ["ProjectCertificates"],
            }),
            getCertificatesByParty: builder.query<ProjectCertificateSummaryDto[], { contractorId?: string; supplierId?: string; certificateType: string }>({
                query: ({ contractorId, supplierId, certificateType }) => ({
                    url: '/project/byParty',
                    params: { contractorId, supplierId, certificateType },
                }),
            }),
            processWorkEffortCertificate: builder.mutation<OrderStatusChangeResult, { workEffortId: string }>({
                query: (data) => ({
                    url: '/facilityInventories/processWorkmanCertificatePurchaseOrder',
                    method: 'POST',
                    body: data,
                }),
                invalidatesTags: ['Certificates'],
            }),
            issueMaterialsForCertificate: builder.mutation({
                query: ({ workEffortId }) => ({
                    url: '/workEffort/issueMaterialsForCertificate',
                    method: 'PUT',
                    body: { workEffortId },
                }),
            }),
        };
    },
});

export const {
    useFetchProjectsQuery,
    useAddProjectMutation,
    useUpdateProjectMutation,
    useFetchProjectCertificatesQuery,
    useAddProjectCertificateMutation,
    useUpdateProjectCertificateMutation,
    useGetCertificatesByPartyQuery,
    useProcessWorkEffortCertificateMutation, useIssueMaterialsForCertificateMutation
} = projectsApi;
export {projectsApi};


interface ProjectCertificateSummaryDto {
    workEffortId: string;
    certificateNumber: string;
    projectId: string;
    projectName: string;
    partyIdSupplier: string | null;
    partyNameSupplier: string | null;
    partyIdContractor: string | null;
    partyNameContractor: string | null;
    description: string | null;
    estimatedStartDate: string | null;
    estimatedCompletionDate: string | null;
    statusDescription: string;
    statusDescriptionArabic: string;
    currentStatusId: string;
    certificateCategory: string;
    certificateCategoryDescription: string;
    facilityId: string | null;
    facilityName: string | null;
    total: number;
}

interface OrderStatusChangeResult {
    orderId: string;
    orderStatusId: string;
    invoiceId: string;
}
