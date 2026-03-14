import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../../configureStore";
import { Lead, LeadLov } from "../../../../features/CRM/models/lead";
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
                    url: `/leads`,
                    method: "POST",
                    body: lead,
                }),
                invalidatesTags: [{ type: "Lead", id: "LIST" }, { type: "Lead", id: "LOV" }],
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
    useUpdateLeadMutation,
} = leadsApi;

export { leadsApi };
