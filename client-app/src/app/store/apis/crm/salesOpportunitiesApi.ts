import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../../configureStore";
import {
    SalesOpportunity,
    OpportunityStage,
    OpportunityQueryParams,
    UpdateStageRequest
} from "../../../../features/CRM/models/salesOpportunity";

/**
 * RTK Query API for Sales Opportunities (Leads/Deals).
 *
 * KEY CONCEPT:
 * A Sales Opportunity (Lead) is NOT a person - it's a business opportunity.
 * People are linked to opportunities via the contacts array.
 */
const salesOpportunitiesApi = createApi({
    reducerPath: "salesOpportunities",
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
    tagTypes: ["SalesOpportunity", "OpportunityStage"],

    endpoints(builder) {
        return {
            // Fetch all opportunities with optional filtering
            fetchOpportunities: builder.query<SalesOpportunity[], OpportunityQueryParams | void>({
                query: (params) => {
                    const searchParams = new URLSearchParams();
                    if (params?.stageId) searchParams.append('stageId', params.stageId);
                    if (params?.ownerPartyId) searchParams.append('ownerPartyId', params.ownerPartyId);
                    if (params?.closeDateFrom) searchParams.append('closeDateFrom', params.closeDateFrom);
                    if (params?.closeDateTo) searchParams.append('closeDateTo', params.closeDateTo);
                    if (params?.search) searchParams.append('search', params.search);
                    if (params?.sortBy) searchParams.append('sortBy', params.sortBy);
                    if (params?.sortDesc !== undefined) searchParams.append('sortDesc', String(params.sortDesc));

                    return {
                        url: `/salesOpportunities?${searchParams.toString()}`,
                        method: "GET",
                    };
                },
                providesTags: (result) =>
                    result
                        ? [
                            ...result.map(({ salesOpportunityId }) => ({
                                type: "SalesOpportunity" as const,
                                id: salesOpportunityId,
                            })),
                            { type: "SalesOpportunity", id: "LIST" },
                        ]
                        : [{ type: "SalesOpportunity", id: "LIST" }],
            }),

            // Fetch pipeline stages for board view
            fetchOpportunityStages: builder.query<OpportunityStage[], void>({
                query: () => ({
                    url: `/salesOpportunities/stages`,
                    method: "GET",
                }),
                providesTags: [{ type: "OpportunityStage", id: "LIST" }],
            }),

            // Create a new opportunity
            createOpportunity: builder.mutation<SalesOpportunity, SalesOpportunity>({
                query: (opportunity) => ({
                    url: `/salesOpportunities`,
                    method: "POST",
                    body: opportunity,
                }),
                invalidatesTags: [{ type: "SalesOpportunity", id: "LIST" }],
            }),

            // Update an existing opportunity
            updateOpportunity: builder.mutation<SalesOpportunity, { id: string; opportunity: SalesOpportunity }>({
                query: ({ id, opportunity }) => ({
                    url: `/salesOpportunities/${id}`,
                    method: "PUT",
                    body: opportunity,
                }),
                invalidatesTags: (result, error, { id }) => [
                    { type: "SalesOpportunity", id },
                    { type: "SalesOpportunity", id: "LIST" },
                ],
            }),

            // Update only the stage (for drag-and-drop on Kanban board)
            updateOpportunityStage: builder.mutation<SalesOpportunity, { id: string; request: UpdateStageRequest }>({
                query: ({ id, request }) => ({
                    url: `/salesOpportunities/${id}/stage`,
                    method: "PATCH",
                    body: request,
                }),
                invalidatesTags: (result, error, { id }) => [
                    { type: "SalesOpportunity", id },
                    { type: "SalesOpportunity", id: "LIST" },
                ],
            }),
        };
    },
});

export const {
    useFetchOpportunitiesQuery,
    useFetchOpportunityStagesQuery,
    useCreateOpportunityMutation,
    useUpdateOpportunityMutation,
    useUpdateOpportunityStageMutation,
} = salesOpportunitiesApi;

export { salesOpportunitiesApi };
