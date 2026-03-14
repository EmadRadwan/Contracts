import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../../configureStore";
import { Lead, LeadLov, LeadQueryParams } from "../../../../features/CRM/models/lead";

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
            // Fetch all leads with optional filtering
            fetchLeads: builder.query<Lead[], LeadQueryParams | void>({
                query: (params) => {
                    const searchParams = new URLSearchParams();
                    if (params?.search) searchParams.append('search', params.search);
                    if (params?.dataSourceId) searchParams.append('dataSourceId', params.dataSourceId);
                    if (params?.sortBy) searchParams.append('sortBy', params.sortBy);
                    if (params?.sortDesc !== undefined) searchParams.append('sortDesc', String(params.sortDesc));

                    return {
                        url: `/leads?${searchParams.toString()}`,
                        method: "GET",
                    };
                },
                providesTags: (result) =>
                    result
                        ? [
                            ...result.map(({ partyId }) => ({
                                type: "Lead" as const,
                                id: partyId,
                            })),
                            { type: "Lead", id: "LIST" },
                        ]
                        : [{ type: "Lead", id: "LIST" }],
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
