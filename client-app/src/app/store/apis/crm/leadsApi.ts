import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../../configureStore";
import { Lead, LeadLov, LeadAssignment, AssignLeadRequest, BulkAssignLeadsRequest, BulkAssignResult, LeadAssignmentHistory } from "../../../../features/CRM/models/lead";
import { State, toODataString } from "@progress/kendo-data-query";

interface ListResponse<T> {
    data: T[];
    total: number;
}

/**
 * RTK Query API for CRM Leads (People).
 */
const leadsApi = createApi({
    reducerPath: "leads",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers) => {
            const token = store.getState().account.user?.token;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            return headers;
        },
    }),
    tagTypes: ["Lead"],

    endpoints(builder) {
        return {
            // Fetch all leads with OData
            fetchLeads: builder.query<ListResponse<Lead>, State>({
                query: (queryArgs) => {
                    const url = `/odata/leadRecords?$count=true&${toODataString(queryArgs)}`;
                    return { url, method: "GET" };
                },
                providesTags: (result) =>
                    result
                        ? [
                            ...result.data.map(({ partyId }) => ({
                                type: "Lead" as const,
                                id: partyId,
                            })),
                            { type: "Lead", id: "LIST" },
                        ]
                        : [{ type: "Lead", id: "LIST" }],
                transformResponse: (response: any, meta) => {
                    const countHeader = meta?.response?.headers.get("count");
                    const totalCount = countHeader ? JSON.parse(countHeader).totalCount : 0;
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),

            // Fetch leads for LOV/picker (lightweight)
            fetchLeadsLov: builder.query<LeadLov[], { search?: string; take?: number } | void>({
                query: (params) => {
                    const searchParams = new URLSearchParams();
                    if (params?.search) searchParams.append('search', params.search);
                    if (params?.take) searchParams.append('take', String(params.take));

                    return {
                        url: `/leads/lov?${searchParams.toString()}`,
                        method: "GET",
                    };
                },
                providesTags: [{ type: "Lead", id: "LOV" }],
            }),

            // Create a new lead
            createLead: builder.mutation<Lead, Lead>({
                query: (lead) => ({
                    url: `/parties/createLead`,
                    method: "POST",
                    body: lead,
                }),
                invalidatesTags: [{ type: "Lead", id: "LIST" }, { type: "Lead", id: "LOV" }],
            }),

            createLeadsBatch: builder.mutation<Lead, Lead[]>({
                query: (leads) => ({
                    url: `/parties/createLeadsBatch`,
                    method: "POST",
                    body: leads,
                }),
                invalidatesTags: [{ type: "Lead", id: "LIST" }, { type: "Lead", id: "LOV" }],
            }),

            // Ownership history for one lead
            fetchLeadAssignmentHistory: builder.query<LeadAssignmentHistory[], string>({
                query: (id) => ({
                    url: `/leads/${id}/assignment-history`,
                    method: "GET",
                }),
                providesTags: (result, error, id) => [{ type: "Lead", id: `ASSIGNMENT-${id}` }],
            }),

            // Assign or reassign a lead to a sales rep
            assignLead: builder.mutation<LeadAssignment, { id: string; request: AssignLeadRequest }>({
                query: ({ id, request }) => ({
                    url: `/leads/${id}/assign`,
                    method: "POST",
                    body: request,
                }),
                invalidatesTags: (result, error, { id }) => [
                    { type: "Lead", id },
                    { type: "Lead", id: `ASSIGNMENT-${id}` },
                    { type: "Lead", id: "LIST" },
                    { type: "Lead", id: "LOV" },
                ],
            }),

            // Assign many leads to one sales rep
            bulkAssignLeads: builder.mutation<BulkAssignResult, BulkAssignLeadsRequest>({
                query: (request) => ({
                    url: `/leads/bulk-assign`,
                    method: "POST",
                    body: request,
                }),
                invalidatesTags: [
                    { type: "Lead", id: "LIST" },
                    { type: "Lead", id: "LOV" },
                ],
            }),

            // Remove the current owner from a lead
            unassignLead: builder.mutation<LeadAssignment, string>({
                query: (id) => ({
                    url: `/leads/${id}/assign`,
                    method: "DELETE",
                }),
                invalidatesTags: (result, error, id) => [
                    { type: "Lead", id },
                    { type: "Lead", id: `ASSIGNMENT-${id}` },
                    { type: "Lead", id: "LIST" },
                    { type: "Lead", id: "LOV" },
                ],
            }),

            // Update an existing lead
            updateLead: builder.mutation<Lead, { id: string; lead: Lead }>({
                query: ({ id, lead }) => ({
                    url: `/leads/${id}`,
                    method: "PUT",
                    body: lead,
                }),
                invalidatesTags: (result, error, { id }) => [
                    { type: "Lead", id },
                    { type: "Lead", id: "LIST" },
                    { type: "Lead", id: "LOV" },
                ],
            }),
        };
    },
});

export const {
    useFetchLeadsQuery,
    useFetchLeadsLovQuery,
    useCreateLeadMutation,
    useCreateLeadsBatchMutation,
    useUpdateLeadMutation,
    useAssignLeadMutation,
    useBulkAssignLeadsMutation,
    useFetchLeadAssignmentHistoryQuery,
    useUnassignLeadMutation,
} = leadsApi;

export { leadsApi };
