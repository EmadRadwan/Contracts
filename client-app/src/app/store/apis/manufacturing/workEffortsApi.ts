import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {State, toODataString} from "@progress/kendo-data-query";
import {store} from "../../configureStore";
import {WorkEffort} from "../../../models/manufacturing/workEffort";
import {ReturnMaterialsRequest, ReturnMaterialsResponse} from "./ReturnItem";
interface ListResponse<T> {
    totalCount: number;
    data: T[];
}

const workEffortsApi = createApi({
    reducerPath: "workEfforts",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            const lang = store.getState().localization.language
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            if (lang) {
                headers.set('Accept-Language', lang)
            }
            return headers;
        },
    }),
    tagTypes: [
        "RoutingTasks",
        "InventoryProduced",
        "ProductionRuns",
        "WorkEffortAssocs",
        "WorkEffort",
        "WorkEffortGoodStandards",
        "IssuedMaterials",
    ],
    endpoints(builder) {
        return {
            fetchProductionRuns: builder.query<ListResponse<WorkEffort>,
                State>({
                query: (queryArgs) => {
                    const url = `/odata/productionRunRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                transformResponse: (response: any, meta, arg) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        totalCount: totalCount,
                    };
                },
                providesTags: ["ProductionRuns"],
            }),
            fetchProductionRunReservations: builder.query<ListResponse<WorkEffort>,
                State>({
                query: (queryArgs) => {
                    const url = `/odata/productionRunReservationRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                transformResponse: (response: any, meta, arg) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        totalCount: totalCount,
                    };
                },
                providesTags: ["ProductionRuns"],
            }),
            fetchRoutings: builder.query<ListResponse<WorkEffort>,
                State>({
                query: (queryArgs) => {
                    const url = `/odata/routingRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                transformResponse: (response: any, meta, arg) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        totalCount: totalCount,
                    };
                },
                providesTags: (result) =>
                    result?.data
                        ? [
                            ...result.data.map((item) => ({ type: "Routings", id: item.workEffortId })),
                            "Routings",
                        ]
                        : ["Routings"],
            }),
            fetchRoutingTasks: builder.query<ListResponse<any>,
                State>({
                query: (queryArgs) => {
                    const url = `/odata/routingTaskRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                transformResponse: (response: any, meta, arg) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        totalCount: totalCount,
                    };
                },
                providesTags: ["RoutingTasks"],
            }),
            fetchProductRoutings: builder.query<WorkEffort[], { productId: string }>({
                query: ({ productId}) => ({
                    url: `/workEffort/${productId}/getProductRoutings`,
                    method: "GET",
                }),
                transformResponse: (response: any, meta) => {
                    return response as WorkEffort[];
                },
            }),
            fetchProductionRunTasks: builder.query<any, any>({
                providesTags: ["RoutingTasks"], 
                query: (workEffortId) => ({
                    url: `/workEffort/${workEffortId}/getRoutingTasksForProductionRun`,
                    method: "GET",
                })
            }),
            fetchProductionRunTasksSimple: builder.query<any, any>({
                providesTags: ["RoutingTasks"], 
                query: (workEffortId) => ({
                    url: `/workEffort/${workEffortId}/getRoutingTasksForProductionRunSimple`,
                    method: "GET",
                })
            }),
            fetchProductionRunMaterials: builder.query<any[], any>({
                query: (workEffortId) => ({
                    url: `/workEffort/${workEffortId}/getProductionRunMaterials`,
                    method: "GET",
                }),
                transformResponse: (response: any, meta) => {
                    return response as any[];
                },
            }),
            fetchIssueProductionRunDeclComponents: builder.query<any[], any>({
                providesTags: ["IssuedMaterials"],
                query: (workEffortId) => ({
                    url: `/workEffort/${workEffortId}/listIssueProductionRunDeclComponents`,
                    method: "GET",
                }),
                transformResponse: (response: any, meta) => {
                    return response as any[];
                },
            }),
            fetchProductionRunComponentsForReturn: builder.query<any[], any>({
                query: (workEffortId) => ({
                    url: `/workEffort/${workEffortId}/getProductionRunComponentsForReturn`,
                    method: "GET",
                }),
                transformResponse: (response: any, meta) => {
                    return response as any[];
                },
            }),
            fetchProducedProductionRunInventory: builder.query<any[], any>({
                query: (workEffortId) => ({
                    url: `/workEffort/${workEffortId}/listProducedProductionRunInventory`,
                    method: "GET",
                }),
                transformResponse: (response: any, meta) => {
                    return response as any[];
                },
                providesTags: ["InventoryProduced"],
            }),
            fetchProductionRunPartyAssignments: builder.query<any[], any>({
                query: (workEffortId) => ({
                    url: `/workEffort/${workEffortId}/getProductionRunParties`,
                    method: "GET",
                }),
                transformResponse: (response: any, meta) => {
                    return response as any[];
                },
            }),
            createProductionRun: builder.mutation({
                invalidatesTags: ["RoutingTasks"],
                query: (productionRun) => {
                    return {
                        url: "/workEffort/createProductionRun",
                        method: "POST",
                        body: {...productionRun},
                    };
                },
                invalidatesTags: ["ProductionRuns"],
            }),
            updateProductionRun: builder.mutation({
                invalidatesTags: ["RoutingTasks"],
                query: (productionRun) => {
                    return {
                        url: "/workEffort/updateProductionRun",
                        method: "PUT",
                        body: {...productionRun},
                    };
                },
                invalidatesTags: ["ProductionRuns"],
            }),
            changeProductionRunStatus: builder.mutation({
                invalidatesTags: ["RoutingTasks"],
                query: (newStatus) => {
                    return {
                        url: "/workEffort/changeProductionRunStatus",
                        method: "PUT",
                        body: {...newStatus},
                    };
                },
            }),
            quickChangeProductionRunStatus: builder.mutation({
                invalidatesTags: ["RoutingTasks"],
                query: (quickStatus) => ({
                    url: "/workEffort/quickChangeProductionRunStatus",
                    method: "PUT",
                    body: { ...quickStatus },
                }),
            }),
            changeProductionRunTaskStatus: builder.mutation({
                invalidatesTags: ["RoutingTasks"],
                query: (newStatus) => {
                    return {
                        url: "/workEffort/changeProductionRunTaskStatus",
                        method: "PUT",
                        body: {...newStatus},
                    };
                },
            }),
            issueProductionRunTask: builder.mutation({
                invalidatesTags: ["RoutingTasks", "IssuedMaterials"],
                query: (newStatus) => {
                    return {
                        url: "/workEffort/issueProductionRunTask",
                        method: "PUT",
                        body: {...newStatus},
                    };
                },
            }),
            reserveProductionRunTask: builder.mutation({
                invalidatesTags: ["RoutingTasks", "IssuedMaterials"],
                query: (newStatus) => {
                    return {
                        url: "/workEffort/reserveProductionRunTask",
                        method: "PUT",
                        body: {...newStatus},
                    };
                },
            }),
            updateProductionRunTask: builder.mutation({
                invalidatesTags: ["RoutingTasks"],
                query: (taskUpdate) => {
                    return {
                        url: "/workEffort/updateProductionRunTask",
                        method: "PUT",
                        body: taskUpdate,
                    };
                },
            }),
            productionRunDeclareAndProduce: builder.mutation({
                invalidatesTags: ["RoutingTasks"],
                query: (declaredQuantityInfo) => {
                    return {
                        url: "/workEffort/declareAndProduceProductionRun",
                        method: "PUT",
                        body: declaredQuantityInfo,
                    };
                },
                invalidatesTags: ["InventoryProduced"],
            }),
            fetchProductionRunWipStatus: builder.query({
                query: ({ mainProductionRunId, finishedProductId }) => `workEffort/${mainProductionRunId}/wip-status?finishedProductId=${finishedProductId}`,
            }),
            receiveProductionRunComponents: builder.mutation<ReturnMaterialsResponse, ReturnMaterialsRequest>({
                query: (request) => ({
                    url: "/workEffort/returnMaterials",
                    method: "POST",
                    body: request,
                }),
                invalidatesTags: ["ProductionRunComponents", "IssuedMaterials"],
            }),
            addRouting: builder.mutation<string, CreateWorkEffortDto>({
                query: (dto) => ({
                    url: '/workEffort/createRouting',
                    method: 'POST',
                    body: dto,
                }),
                transformResponse: (response: { workEffortId: string }) => response.workEffortId,
                transformErrorResponse: (response: { data: { error: string } }) => response.data.error,
                invalidatesTags: ['WorkEffort'],
            }),
            updateRouting: builder.mutation<string, UpdateRoutingDto>({
                query: (dto) => ({
                    url: '/workEffort/updateRouting',
                    method: 'POST',
                    body: dto,
                }),
                transformResponse: (response: { workEffortId: string }) => response.workEffortId,
                transformErrorResponse: (response: { data: { error: string } }) => response.data.error,
                invalidatesTags: ['WorkEffort'],
            }),
            addRoutingTask: builder.mutation<WorkEffortResponse, CreateRoutingTaskDto>({
                query: (dto) => ({
                    url: 'workeffort/createRoutingTask',
                    method: 'POST',
                    body: dto,
                }),
                invalidatesTags: ['RoutingTasks'],
            }),

            updateRoutingTask: builder.mutation<WorkEffortResponse, UpdateRoutingTaskDto>({
                query: (dto) => ({
                    url: 'workeffort/updateRoutingTask',
                    method: 'POST',
                    body: dto,
                }),
                invalidatesTags: ['RoutingTasks'],
            }),
            createWorkEffortAssoc: builder.mutation<CreateWorkEffortAssocResponse, WorkEffortAssocDto>({
                query: (workEffortAssoc) => ({
                    url: '/WorkEffort/createWorkEffortAssoc',
                    method: 'POST',
                    body: {
                        workEffortIdFrom: workEffortAssoc.workEffortIdFrom,
                        workEffortIdTo: workEffortAssoc.workEffortIdTo,
                        workEffortAssocTypeId: workEffortAssoc.workEffortAssocTypeId,
                        fromDate: workEffortAssoc.fromDate, // Sent as ISO string or undefined
                        thruDate: workEffortAssoc.thruDate, // Sent as ISO string or undefined
                        sequenceNum: workEffortAssoc.sequenceNum, // Sent as number or undefined
                    },
                }),
                invalidatesTags: (result, error, workEffortAssoc) => [
                    { type: 'WorkEffortAssocs', id: workEffortAssoc.workEffortIdFrom },
                    { type: 'WorkEffortAssocs', id: workEffortAssoc.workEffortIdTo },
                ],
            }),
            getRoutingTaskAssocs: builder.query<GetRoutingTaskAssocsResponse, string>({
                query: (workEffortId) => ({
                    url: `/WorkEffort/getRoutingTaskAssocs/${workEffortId}`,
                    method: 'GET',
                }),
                providesTags: (result, error, workEffortId) =>
                    result?.data
                        ? [
                            ...result.data.flatMap((assoc: WorkEffortAssocDto) => [
                                { type: 'WorkEffortAssocs', id: assoc.workEffortIdFrom },
                                { type: 'WorkEffortAssocs', id: assoc.workEffortIdTo },
                            ]),
                            { type: 'WorkEffortAssocs', id: workEffortId },
                        ]
                        : [{ type: 'WorkEffortAssocs', id: workEffortId }],
            }),
            deleteWorkEffortAssoc: builder.mutation<void, DeleteWorkEffortAssocPayload>({
                query: (payload) => ({
                    url: '/WorkEffort/delete',
                    method: 'POST',
                    body: payload,
                }),
                // Business: Ensures the frontend reflects the updated list of associations
                // Technical: Invalidates the cache for the specific WorkEffortId to trigger a refetch
                invalidatesTags: (result, error, { workEffortIdFrom, workEffortIdTo }) => [
                    { type: 'WorkEffortAssocs', id: workEffortIdFrom },
                    { type: 'WorkEffortAssocs', id: workEffortIdTo },
                ],
            }),
            updateWorkEffortAssoc: builder.mutation<void, WorkEffortAssocDto>({
                query: (workEffortAssoc) => ({
                    url: '/WorkEffort/updateWorkEffortAssoc',
                    method: 'PUT',
                    body: {
                        workEffortIdFrom: workEffortAssoc.workEffortIdFrom,
                        workEffortIdTo: workEffortAssoc.workEffortIdTo,
                        workEffortAssocTypeId: workEffortAssoc.workEffortAssocTypeId,
                        fromDate: workEffortAssoc.fromDate,
                        thruDate: workEffortAssoc.thruDate,
                        sequenceNum: workEffortAssoc.sequenceNum,
                    },
                }),
                invalidatesTags: (result, error, workEffortAssoc) => [
                    { type: 'WorkEffortAssocs', id: workEffortAssoc.workEffortIdFrom },
                    { type: 'WorkEffortAssocs', id: workEffortAssoc.workEffortIdTo },
                ],
            }),
            createRoutingProductLink: builder.mutation<void, WorkEffortGoodStandardDto>({
                query: (workEffortGoodStandard) => ({
                    url: '/WorkEffort/createWorkEffortGoodStandard',
                    method: 'POST',
                    body: {
                        workEffortId: workEffortGoodStandard.workEffortId,
                        productId: workEffortGoodStandard.productId,
                        workEffortGoodStdTypeId: workEffortGoodStandard.workEffortGoodStdTypeId,
                        fromDate: workEffortGoodStandard.fromDate,
                        estimatedQuantity: workEffortGoodStandard.estimatedQuantity,
                    },
                }),
                // Business: Ensures the frontend reflects the updated list after creation
                // Technical: Invalidates cache for the specific WorkEffortId
                invalidatesTags: (result, error, workEffortGoodStandard) => [
                    { type: 'WorkEffortGoodStandards', id: workEffortGoodStandard.workEffortId },
                ],
            }),
            updateRoutingProductLink: builder.mutation<void, WorkEffortGoodStandardDto>({
                query: (workEffortGoodStandard) => ({
                    url: '/WorkEffort/updateWorkEffortGoodStandard',
                    method: 'PUT',
                    body: {
                        workEffortId: workEffortGoodStandard.workEffortId,
                        productId: workEffortGoodStandard.productId,
                        workEffortGoodStdTypeId: workEffortGoodStandard.workEffortGoodStdTypeId,
                        fromDate: workEffortGoodStandard.fromDate,
                        thruDate: workEffortGoodStandard.thruDate,
                        estimatedQuantity: workEffortGoodStandard.estimatedQuantity,
                    },
                }),
                // Business: Ensures the frontend reflects the updated list after update
                // Technical: Invalidates cache for the specific WorkEffortId
                invalidatesTags: (result, error, workEffortGoodStandard) => [
                    { type: 'WorkEffortGoodStandards', id: workEffortGoodStandard.workEffortId },
                ],
            }),
            deleteRoutingProductLink: builder.mutation<void, { workEffortId: string; productId: string; workEffortGoodStdTypeId: string; fromDate: string }>({
                query: (payload) => ({
                    url: '/WorkEffort/deleteWorkEffortGoodStandard',
                    method: 'POST',
                    body: payload,
                }),
                // Business: Ensures the frontend reflects the updated list after deletion
                // Technical: Invalidates cache for the specific WorkEffortId
                invalidatesTags: (result, error, { workEffortId }) => [
                    { type: 'WorkEffortGoodStandards', id: workEffortId },
                ],
            }),
            getRoutingProductLinks: builder.query<WorkEffortGoodStandardListDto[], string>({
                query: (workEffortId) => ({
                    url: `/WorkEffort/getRoutingProductLinks/${workEffortId}`,
                    method: 'GET',
                }),
                // Business: Ensures the frontend cache is updated when related data changes
                // Technical: Tags the query with WorkEffortId for cache invalidation
                providesTags: (result, error, workEffortId) => [
                    { type: 'WorkEffortGoodStandards', id: workEffortId },
                ],
            }),
            getWorkEffort: builder.query<WorkEffortDto, string>({
                query: (workEffortId) => `/workEffort/${workEffortId}`,
                providesTags: ['WorkEffort'],
            }),
            getBomInventoryItems: builder.query<BomInventoryItem[], string>({
                query: (workEffortId) => `/workEffort/${workEffortId}/inventoryItems`,
            }),
        };
    },
});

export const {
    useFetchRoutingsQuery, useFetchRoutingTasksQuery, useFetchProductRoutingsQuery,
    useCreateProductionRunMutation, useFetchProductionRunsQuery,
    useFetchProductionRunTasksQuery, useFetchProductionRunMaterialsQuery, 
    useChangeProductionRunStatusMutation, useChangeProductionRunTaskStatusMutation,
    useFetchProductionRunPartyAssignmentsQuery, useIssueProductionRunTaskMutation,
    useUpdateProductionRunTaskMutation, useProductionRunDeclareAndProduceMutation, 
    useUpdateProductionRunMutation, useFetchProductionRunTasksSimpleQuery, useReserveProductionRunTaskMutation,
    useFetchIssueProductionRunDeclComponentsQuery, useFetchProductionRunComponentsForReturnQuery, useFetchProducedProductionRunInventoryQuery,
    useFetchProductionRunReservationsQuery, useQuickChangeProductionRunStatusMutation,
    useFetchProductionRunWipStatusQuery, useReceiveProductionRunComponentsMutation,
    useAddRoutingMutation, useUpdateRoutingMutation, useAddRoutingTaskMutation,
    useUpdateRoutingTaskMutation, 
    useCreateWorkEffortAssocMutation, useGetRoutingTaskAssocsQuery,
    useDeleteWorkEffortAssocMutation, useUpdateWorkEffortAssocMutation,
    useCreateRoutingProductLinkMutation, useUpdateRoutingProductLinkMutation, 
    useDeleteRoutingProductLinkMutation,
    useGetRoutingProductLinksQuery, useGetWorkEffortQuery, useGetBomInventoryItemsQuery
} = workEffortsApi;
export {workEffortsApi};
