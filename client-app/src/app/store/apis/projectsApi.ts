import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {State, toODataString} from "@progress/kendo-data-query";
import {WorkEffort} from "../../models/manufacturing/workEffort";
import {CertificateStatus} from "../../../features/Projects/hook/useProjectCertificate";
import {Payment} from "../../models/accounting/payment";
import {ProjectCommissionRate} from "../../models/orders/projectCommissionRate";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const REVIEW_CERTIFICATE_TAG = "ReviewCertificate" as const;
const COMMISSION_RATE_TAG = "ProjectCommissionRate" as const;


const projectsApi = createApi({
    reducerPath: "projects",
    tagTypes: ["WorkEffort", "ProjectCertificates", REVIEW_CERTIFICATE_TAG, COMMISSION_RATE_TAG],
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
    refetchOnMountOrArgChange: true,
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
                    url: `project/updateProject`, // Updated to match backend route
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
            approvePOForCertificate: builder.mutation<void, string>({
                query: (workEffortId) => ({
                    url: `/project/${workEffortId}/approve-po`,
                    method: "POST",
                }),
                invalidatesTags: ["ProjectCertificates", "ProjectCertificate"], // Recommended: refresh list + detail
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
                invalidatesTags: ['ProjectCertificates']
            }),
            issueMaterialsForCertificate: builder.mutation({
                query: ({ workEffortId }) => ({
                    url: '/workEffort/issueMaterialsForCertificate',
                    method: 'PUT',
                    body: { workEffortId },
                }),
                invalidatesTags: ['ProjectCertificates']
            }),
            deleteProjectCertificate: builder.mutation<void, string>({
                query: (workEffortId) => ({
                    url: `/project/deleteProjectCertificate/${workEffortId}`,
                    method: 'DELETE',
                }),
                invalidatesTags: ['ProjectCertificates'], // or whatever tag you use for list invalidation
            }),
            fetchProjectCertificatesByDateRange: builder.query<WorkEffort[], { startDate?: string; endDate?: string }>({
                query: ({ startDate, endDate }) => ({
                    url: "/project/by-date-range",
                    params: { startDate, endDate },
                }),
            }),
            reviewCertificate: builder.mutation<
                { success: boolean; certificate?: any },
                {
                    workEffortId: string;
                    status: CertificateStatus;
                    comments?: string; // ← NEW: optional comments field
                }
                >({
                query: ({ workEffortId, status, comments }) => ({
                    url: `project/review`,
                    method: "POST",
                    body: {
                        workEffortId,
                        newStatusId: status,
                        comments: comments?.trim() || undefined, // send only if provided and non-empty
                    },
                }),
                // Invalidate relevant caches to ensure UI refreshes with latest status and potential comment history
                invalidatesTags: [
                    "ProjectCertificates",
                    "ProjectCertificate",
                    "CertificateItems",
                    REVIEW_CERTIFICATE_TAG,
                ],
            }),
            resetProjectCertificate: builder.mutation<void, string>({
                query: (workEffortId) => ({
                    url: `/project/resetCertificate/${workEffortId}`,
                    method: 'POST',
                }),
                invalidatesTags: ['ProjectCertificates', 'ProjectCertificate'],
            }),
            fetchWorkEffortsByGlAccountId: builder.query<WorkEffort[], { glAccountId: string; workEffortTypeId?: string; workEffortParentId?: string }>({
                query: ({ glAccountId, workEffortTypeId, workEffortParentId }) => ({
                    url: `/project/workEffortsByGlAccount`,
                    params: { glAccountId, workEffortTypeId, workEffortParentId },
                }),
            }),
            fetchProjectReport: builder.query<ProjectReportDto, { 
                projectId: string; 
                expensesStartDate?: string; 
                expensesEndDate?: string; 
                expensesAllData: boolean;
                revenuesStartDate?: string; 
                revenuesEndDate?: string; 
                revenuesAllData: boolean;
            }>({
                query: (params) => ({
                    url: "/project/report",
                    params,
                }),
            }),
            fetchCompanyReport: builder.query<ProjectReportDto, {
                expensesStartDate?: string;
                expensesEndDate?: string;
                expensesAllData: boolean;
                revenuesStartDate?: string;
                revenuesEndDate?: string;
                revenuesAllData: boolean;
            }>({
                query: (params) => ({
                    url: "/project/companyReport",
                    params,
                }),
            }),
            fetchProjectCommissionRates: builder.query<ListResponse<ProjectCommissionRate>, State>({
                query: (queryArgs) => ({
                    url: `/odata/ProjectCommissionRateRecords?$count=true&${toODataString(queryArgs)}`,
                    method: "GET",
                }),
                transformResponse: (response: any, meta) => {
                    const { totalCount } = JSON.parse(meta!.response!.headers.get("count")!);
                    return { data: response, total: totalCount };
                },
                providesTags: [COMMISSION_RATE_TAG],
            }),
            addProjectCommissionRate: builder.mutation<ProjectCommissionRate, Partial<ProjectCommissionRate>>({
                query: (dto) => ({
                    url: "/project/createProjectCommissionRate",
                    method: "POST",
                    body: { ...dto },
                }),
                invalidatesTags: [COMMISSION_RATE_TAG],
            }),
            updateProjectCommissionRate: builder.mutation<ProjectCommissionRate, Partial<ProjectCommissionRate>>({
                query: (dto) => ({
                    url: `/project/updateProjectCommissionRate/${dto.projectCommissionRateId}`,
                    method: "PUT",
                    body: { ...dto },
                }),
                invalidatesTags: [COMMISSION_RATE_TAG],
            }),
            fetchProjectsLov: builder.query<{ projects: ProjectLovItem[]; projectCount: number }, void>({
                query: () => ({
                    url: "/project/getProjectsLov",
                    params: { pageSize: 1000 },
                }),
            }),
            fetchProjectCertificatesForExport: builder.query<WorkEffort[], { oDataQuery: string; fromDate?: string; toDate?: string }>({
                query: ({ oDataQuery, fromDate, toDate }) => {
                    let url = `/odata/ProjectCertificateRecords?${oDataQuery}`;
                    if (fromDate) url += `&fromDate=${fromDate}`;
                    if (toDate) url += `&toDate=${toDate}`;
                    return { url, method: "GET" };
                },
                transformResponse: (response: any) =>
                    Array.isArray(response) ? response : (response?.value ?? []),
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
    useProcessWorkEffortCertificateMutation,
    useIssueMaterialsForCertificateMutation,
    useReviewCertificateMutation,
    useApprovePOForCertificateMutation,
    useDeleteProjectCertificateMutation,
    useResetProjectCertificateMutation,
    useFetchWorkEffortsByGlAccountIdQuery,
    useLazyFetchProjectReportQuery,
    useLazyFetchCompanyReportQuery,
    useFetchProjectCommissionRatesQuery,
    useAddProjectCommissionRateMutation,
    useUpdateProjectCommissionRateMutation,
    useFetchProjectsLovQuery,
    useLazyFetchProjectCertificatesForExportQuery,
} = projectsApi;
export {projectsApi};


export interface ProjectLovItem {
    workEffortId: string;
    projectName: string;
    facilityId: string;
}

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

export interface ProjectReportDto {
    expenses: ProjectExpenseRecord[];
    revenues: ProjectRevenueRecord[];
    directPayments: Payment[];
    operatingExpenses: Payment[];
    accountingTransactions: Payment[];
}

export interface ProjectExpenseRecord {
    expenseItemKey?: string;
    certificateKey?: string;
    certificateNumber?: string;
    paymentId?: string;
    projectId?: string;
    partyId?: string;
    partyName?: string;
    partyRole?: string;
    productId?: string;
    productName?: string;
    expenseDate?: string;
    recordType?: string;
    certificateType?: string;
    certificateCategoryCode?: string;
    certificateDescription?: string;
    itemDescription?: string;
    relatedPurchaseOrderId?: string;
    isSupplyProcurement: boolean;
    isWorkmanship: boolean;
    isMultiPaymentCertificate: boolean;
    quantity: number;
    unitRate?: number;
    grossAmount: number;
    discountAmount: number;
    deductionsAmount: number;
    insuranceAmount: number;
    transportationExpensesAmount: number;
    gratuitiesAmount: number;
    netCertifiedAmount: number;
    achievementPercentage: number;
    certificateTypeArabic?: string;
}

export interface ProjectRevenueRecord {
    paymentId?: string;
    salesRequestId?: string;
    apartmentId?: string;
    buildingNumber?: string;
    projectId?: string;
    projectName?: string;
    customerPartyId?: string;
    customerName?: string;
    paymentTypeId?: string;
    paymentTypeArabic?: string;
    revenueCategory?: string;
    scheduledAmount: number;
    collectedAmount: number;
    outstandingAmount: number;
    lateAmount: number;
    futureAmount: number;
    paymentStatus?: string;
    overdueBucket?: string;
    daysOverdue: number;
    dueDate?: string;
    createdDate?: string;
    comments?: string;
    chequeNumber?: string;
    dueStatusArabic?: string;
    year?: number;
    quarter?: string;
}
