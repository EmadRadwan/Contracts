import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {State, toODataString} from "@progress/kendo-data-query";
import {SalesCommission, CommissionDefaults, ApprovedSalesRequestEnvelope} from "../../models/orders/salesCommission";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const COMMISSION_TAG = "SalesCommission" as const;

const salesCommissionsApi = createApi({
    reducerPath: "salesCommissions",
    tagTypes: [COMMISSION_TAG],
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers) => {
            const token = store.getState().account.user?.token;
            const lang = store.getState().localization.language;
            if (token) headers.set("authorization", `Bearer ${token}`);
            if (lang) headers.set("Accept-Language", lang);
            return headers;
        },
    }),
    refetchOnMountOrArgChange: true,
    endpoints(builder) {
        return {
            fetchSalesCommissions: builder.query<ListResponse<SalesCommission>, State>({
                query: (queryArgs) => ({
                    url: `/odata/SalesCommissionRecords?$count=true&${toODataString(queryArgs)}`,
                    method: "GET",
                }),
                transformResponse: (response: any, meta) => {
                    const {totalCount} = JSON.parse(meta!.response!.headers.get("count")!);
                    const data = (response as any[]).map((item) => ({
                        ...item,
                        commissionDate: item.commissionDate ? new Date(item.commissionDate) : null,
                        createdStamp: item.createdStamp ? new Date(item.createdStamp) : null,
                        lastUpdatedStamp: item.lastUpdatedStamp ? new Date(item.lastUpdatedStamp) : null,
                    }));
                    return { data, total: Number(totalCount) };
                },
                providesTags: [COMMISSION_TAG],
            }),
            getSalesCommissionBySalesRequest: builder.query<SalesCommission | null, string>({
                query: (salesRequestId) => ({
                    url: `/salesCommission/bySalesRequest/${salesRequestId}`,
                }),
                providesTags: [COMMISSION_TAG],
            }),
            getSalesCommissionDefaults: builder.query<CommissionDefaults, { salesRequestId: string; saleTypeId?: string | null }>({
                query: ({ salesRequestId, saleTypeId }) => ({
                    url: `/salesCommission/defaults/${salesRequestId}`,
                    params: saleTypeId ? { saleTypeId } : undefined,
                }),
            }),
            createSalesCommission: builder.mutation<SalesCommission, Partial<SalesCommission>>({
                query: (dto) => ({
                    url: "/salesCommission/create",
                    method: "POST",
                    body: dto,
                }),
                invalidatesTags: [COMMISSION_TAG],
            }),
            updateSalesCommission: builder.mutation<SalesCommission, { id: string; dto: Partial<SalesCommission> }>({
                query: ({ id, dto }) => ({
                    url: `/salesCommission/${id}`,
                    method: "PUT",
                    body: dto,
                }),
                invalidatesTags: [COMMISSION_TAG],
            }),
            approveSalesCommission: builder.mutation<void, string>({
                query: (id) => ({
                    url: `/salesCommission/approve/${id}`,
                    method: "POST",
                }),
                invalidatesTags: [COMMISSION_TAG],
            }),
            resetSalesCommission: builder.mutation<void, string>({
                query: (id) => ({
                    url: `/salesCommission/reset/${id}`,
                    method: "POST",
                }),
                invalidatesTags: [COMMISSION_TAG],
            }),
            deleteSalesCommission: builder.mutation<void, string>({
                query: (id) => ({
                    url: `/salesCommission/${id}`,
                    method: "DELETE",
                }),
                invalidatesTags: [COMMISSION_TAG],
            }),
            fetchApprovedSalesRequestsLov: builder.query<ApprovedSalesRequestEnvelope, { skip: number; pageSize: number; searchTerm?: string }>({
                query: ({ skip, pageSize, searchTerm }) => ({
                    url: "/salesRequests/approvedLov",
                    params: { skip, pageSize, ...(searchTerm ? { searchTerm } : {}) },
                }),
            }),
            fetchSalesCommissionsForExport: builder.query<SalesCommission[], string>({
                query: (oDataQuery) => ({
                    url: `/odata/SalesCommissionRecords?${oDataQuery}`,
                    method: "GET",
                }),
                transformResponse: (response: any) =>
                    Array.isArray(response) ? response : (response?.value ?? []),
            }),
        };
    },
});

export const {
    useFetchSalesCommissionsQuery,
    useGetSalesCommissionBySalesRequestQuery,
    useGetSalesCommissionDefaultsQuery,
    useCreateSalesCommissionMutation,
    useUpdateSalesCommissionMutation,
    useApproveSalesCommissionMutation,
    useResetSalesCommissionMutation,
    useDeleteSalesCommissionMutation,
    useFetchApprovedSalesRequestsLovQuery,
    useLazyFetchSalesCommissionsForExportQuery,
} = salesCommissionsApi;

export {salesCommissionsApi};
