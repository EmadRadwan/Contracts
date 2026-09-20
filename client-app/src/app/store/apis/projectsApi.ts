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
                salesStartDate?: string;
                salesEndDate?: string;
                salesAllData: boolean;
                commissionsStartDate?: string;
                commissionsEndDate?: string;
                commissionsAllData: boolean;
                mgmtFeeStartDate?: string;
                mgmtFeeEndDate?: string;
                mgmtFeeAllData: boolean;
                // Management-fee inputs — drive ProjectReportDto.summary server-side.
                // excludedBuildings is comma-separated (e.g. "A1,A2").
                mgmtFeePercent?: number;
                excludedBuildings?: string;
            }>({
                query: (params) => ({
                    url: "/project/report",
                    params,
                }),
            }),
            // Server-rendered PDF (Telerik Reporting) — replaces a KendoReact Grid PDFExport attempt
            // that could not shape/reorder Arabic text. Same params as fetchProjectReport, plus the
            // display name the DTO itself doesn't carry.
            fetchProjectReportPdf: builder.query<ArrayBuffer, {
                projectId: string;
                projectName: string;
                expensesStartDate?: string;
                expensesEndDate?: string;
                expensesAllData: boolean;
                revenuesStartDate?: string;
                revenuesEndDate?: string;
                revenuesAllData: boolean;
                salesStartDate?: string;
                salesEndDate?: string;
                salesAllData: boolean;
                commissionsStartDate?: string;
                commissionsEndDate?: string;
                commissionsAllData: boolean;
                mgmtFeeStartDate?: string;
                mgmtFeeEndDate?: string;
                mgmtFeeAllData: boolean;
                mgmtFeePercent?: number;
                excludedBuildings?: string;
            }>({
                query: (params) => ({
                    url: "/project/report/pdf",
                    params,
                    responseHandler: (response) => response.arrayBuffer(),
                    cache: "no-cache",
                }),
            }),
            fetchProjectBuildings: builder.query<string[], string>({
                query: (projectId) => ({
                    url: "/project/buildings",
                    params: { projectId },
                }),
            }),
            fetchCompanyReport: builder.query<ProjectReportDto, {
                expensesStartDate?: string;
                expensesEndDate?: string;
                expensesAllData: boolean;
                revenuesStartDate?: string;
                revenuesEndDate?: string;
                revenuesAllData: boolean;
                salesStartDate?: string;
                salesEndDate?: string;
                salesAllData: boolean;
                mgmtFeePercent?: number;
                excludedBuildings?: string;
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
    useLazyFetchProjectReportPdfQuery,
    useLazyFetchCompanyReportQuery,
    useFetchProjectBuildingsQuery,
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
    payroll: Payment[];
    apartmentSales: any[];
    // One row per sold unit — agreed deposit from the sales request vs. collected receipts.
    maintenanceDeposits: ProjectMaintenanceDepositRecord[];
    paidCommissions: ProjectCommissionPaymentRecord[];
    // Ledger entries (AcctgTransEntry) of every AcctgTrans linked to a payment in paidCommissions.
    paidCommissionAcctgEntries?: ProjectCommissionAcctgEntryRecord[];
    // Server-computed roll-ups (mirror of ProjectReportSummaryDto). Authoritative — the in-app
    // report screen and the Excel/PDF exports read these instead of re-summing in the browser.
    // Optional while the old Excel dialog still computes its own totals (removed in a later step).
    summary?: ProjectReportSummary;
}

export interface ProjectMaintenanceDepositRecord {
    salesRequestId: string;
    apartmentId?: string;
    apartmentName?: string;
    buildingNumber?: string;
    floorNumber?: string;
    customerPartyId?: string;
    customerName?: string;
    saleDate?: string;
    totalPrice?: number;
    maintenancePercent?: number;   // fraction (0.08 = 8%)
    maintenanceDeposit: number;
    collectedAmount: number;
    outstandingAmount: number;
    receiptCount: number;
    receivedCount: number;
    nextDueDate?: string;
    isFullyCollected: boolean;
    collectionStatusArabic: string;
    dueStatusArabic?: string;
}

export interface ProjectReportSummary {
    // المصاريف
    certificateExpenses: number;
    directPayments: number;          // = directPaymentsPaid + directPaymentsUnpaid
    directPaymentsPaid: number;      // PMNT_SENT / PMNT_CONFIRMED
    directPaymentsUnpaid: number;    // PMNT_NOT_PAID — open commitments, shown regardless of period
    directPaymentsPaidCount: number;
    directPaymentsUnpaidCount: number;
    accountingTransactions: number;
    projectPayroll: number;
    operatingExpenses: number;
    totalProjectExpenses: number;
    // الإيرادات / وديعة الصيانة
    revenueScheduled: number;
    revenueCollected: number;
    revenueOutstanding: number;
    maintenanceScheduled: number;
    maintenanceCollected: number;
    maintenanceOutstanding: number;
    maintenanceUnits: number;            // sold units carrying a deposit
    maintenanceUnitsCollected: number;   // of which fully collected
    maintenanceReceiptsCount: number;     // maintenance receipts inside the revenue window
    maintenanceReceiptsScheduled: number;
    maintenanceReceiptsCollected: number;
    // مبيعات الوحدات
    unitsSold: number;
    unitsSoldValue: number;
    unitsAdvanceCollected: number;
    unitsAvailable: number;
    // العمولات
    commissionPaymentCount: number;
    commissionsPaid: number;
    commissionsPending: number;
    // مبلغ الإدارة
    mgmtFeeBase: number;
    mgmtFeePercent: number;
    mgmtFee: number;
    mgmtFeeOperatingExpenses: number;   // operating expenses deducted, over the fee's own window
    mgmtFeeNet: number;
    mgmtExcludedBuildings: string[];
    // الصافي
    netAfterExpenses: number;
    netAfterPaidCommissions: number;
}

export interface ProjectCommissionPaymentRecord {
    paymentId: string;
    salesCommissionId?: string;
    salesRequestId?: string;
    saleTypeId?: string;
    commissionStatusId?: string;
    commissionStatusArabic?: string;
    apartmentId?: string;
    apartmentName?: string;
    buildingNumber?: string;
    payeePartyId?: string;
    payeeName?: string;
    amount: number;
    isPaid: boolean;
    paymentStatusId?: string;
    paymentStatusArabic?: string;
    paymentMethodTypeArabic?: string;
    effectiveDate?: string;
    createdStamp?: string;
    chequeNumber?: string;
    chequeDate?: string;
    comments?: string;
    costCenterId?: string;
    costCenterDescription?: string;
    overrideGlAccountId?: string;
    overrideGlAccountCode?: string;
    overrideGlAccountNameArabic?: string;
}

export interface ProjectCommissionAcctgEntryRecord {
    paymentId: string;
    salesCommissionId?: string;
    salesRequestId?: string;
    apartmentName?: string;
    payeeName?: string;
    acctgTransId: string;
    acctgTransTypeId?: string;
    acctgTransTypeDescription?: string;
    transactionDate?: string;
    isPosted?: string;
    postedDate?: string;
    glFiscalTypeId?: string;
    transDescription?: string;
    costCenterId?: string;
    costCenterDescription?: string;
    acctgTransEntrySeqId?: string;
    glAccountId?: string;
    accountCode?: string;
    accountName?: string;
    accountNameArabic?: string;
    glAccountTypeId?: string;
    debitCreditFlag?: string;
    debit: number;
    credit: number;
    entryPartyId?: string;
    entryPartyName?: string;
    entryDescription?: string;
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
    deservedToday?: string;
    deservedWithinWeek?: string;
    deservedWithinMonth?: string;
    lateDue?: string;
    year?: number;
    quarter?: string;
}
