import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {State, toODataString} from "@progress/kendo-data-query";
import {store} from "../../configureStore";
import {CostComponentCalc} from "../../../models/manufacturing/costComponentCalc";
import {CostComponent} from "../../../models/manufacturing/costComponent";
import {BOMSimulation} from "../../../models/manufacturing/bomSimulation";
import {MaterialCost} from "../../../models/manufacturing/materialCost";
import {RoutingFixedAssetCost} from "../../../models/manufacturing/routingFixedAssetCost";
import {ActualProductCostComponentsTotalsDto} from "./ActualProductCostComponentsTotalsDto";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const costsApi = createApi({
    reducerPath: "costs",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            return headers;
        },
    }),
    tagTypes: ["CostComponents", "CostComponentCalc", "MaterialCost", "RoutingFixedAssetCost", "RoutingCost", "BOMSimulation"],
    endpoints(builder) {
        return {
            fetchCostComponents: builder.query<ListResponse<CostComponent>, any>({
                providesTags: ['CostComponents'],
                query: (productId) => {
                    return {
                        url: `/costs/${productId}/getProductCostComponents`,
                        method: "GET"
                    }
                },
                transformResponse: (response: any) => {
                    return {
                        data: response,
                        total: response.length,
                    };
                }
            }),
            fetchActualCostComponents: builder.query<ActualProductCostComponentsTotalsDto, string>({
                providesTags: ['CostComponents'],
                query: (productionRunId) => ({
                    url: `/costs/${productionRunId}/getActualProductCostComponents`,
                    method: "GET"
                }),
                transformResponse: (response: ActualProductCostComponentsTotalsDto) => {
                    // Group by costComponentTypeId for tree view
                    const groupedData = response.costComponents.reduce((acc: any, row: CostComponent) => {
                        const key = row.costComponentTypeId;
                        const description = row.costComponentTypeDescription;

                        if (!acc[key]) {
                            acc[key] = {
                                costComponentTypeId: key,
                                costComponentTypeDescription: description,
                                costUomId: row.costUomId,
                                cost: 0,
                                children: [],
                            };
                        }

                        acc[key].cost += row.cost || 0;
                        acc[key].children.push(row);

                        return acc;
                    }, {});

                    return {
                        ...response,
                        costComponents: Object.values(groupedData),
                        total: Object.values(groupedData).length
                    };
                }
            }),
            fetchCostComponentCalcs: builder.query<ListResponse<CostComponentCalc>, State>({
                query: (queryArgs) => {
                    const url = `/odata/costComponentCalcRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                transformResponse: (response: any, meta) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
                providesTags: ['CostComponentCalc'],
            }),
            fetchProductCostComponentCalcs: builder.query<ListResponse<CostComponentCalc>, any>({
                query: (productId) => {
                    return {
                        url: `/costs/${productId}/getProductCostComponentCalcs`,
                        method: "GET"
                    }
                },
                transformResponse: (response: any) => {
                    return {
                        data: response,
                        total: response.length,
                    };
                },
            }),
            fetchMaterialCost: builder.query<ListResponse<MaterialCost>, any>({
                query: (productId) => {
                    return {
                        url: `/costs/${productId}/getMaterialCostConfig`,
                        method: "GET"
                    }
                },
                transformResponse: (response: any) => {
                    return {
                        data: response,
                        total: response.length
                    };
                },
            }),
            fetchLaborCost: builder.query<ListResponse<CostComponentCalc>, any>({
                query: (productId) => {
                    return {
                        url: `/costs/${productId}/listTaskDirectLaborCalculations`,
                        method: "GET"
                    }
                },
                transformResponse: (response: any) => {
                    const total = new Set(response.map((r: CostComponentCalc) => r.costComponentTypeId)).size
                    return {
                        data: response,
                        total,
                    };
                },
            }),
            fetchFOHCost: builder.query<ListResponse<CostComponentCalc>, any>({
                query: (productId) => {
                    return {
                        url: `/costs/${productId}/listTaskFOHCalculations`,
                        method: "GET"
                    }
                },
                transformResponse: (response: any) => {
                    return {
                        data: response,
                        total: response.length // Set total to the number of records
                    };
                },
            }),
            fetchFixedAssetCost: builder.query<ListResponse<RoutingFixedAssetCost>, any>({
                query: (productId) => {
                    return {
                        url: `/costs/${productId}/getRoutingTaskFixedAssetCosts`,
                        method: "GET"
                    }
                },
                transformResponse: (response: any) => {
                    return {
                        data: response,
                        total: response.length,
                    };
                },
            }),
            fetchRoutingCost: builder.query<ListResponse<CostComponentCalc>, any>({
                query: (productId) => {
                    return {
                        url: `/costs/${productId}/getRoutingTaskCostComponentCalcs`,
                        method: "GET"
                    }
                },
                transformResponse: (response: any) => {
                    return {
                        data: response,
                        total: response.length,
                    };
                },
            }),
            fetchSimulatedBomCost: builder.query<BOMSimulation[], { productId: string, quantityToProduce: number, currencyUomId: string }>({
                query: ({productId, quantityToProduce, currencyUomId}) => ({
                    url: `/bom/${productId}/${quantityToProduce}/${currencyUomId}/getSimulatedBomCost`,
                    method: "GET",
                }),
                transformResponse: (response: any) => {
                    return response as BOMSimulation[];
                },
            }),
            calculateProductCosts: builder.mutation({
                invalidatesTags: ["CostComponents"],
                query: (productId) => {
                    return {
                        url: `/products/${productId}/calculateProductCosts`,
                        method: 'GET',
                    };
                },
            }),
            addCostComponent: builder.mutation<{ costComponentId: string }, CostComponentDto>({
                query: (dto) => ({
                    url: '/workEffort/createCostComponent',
                    method: 'POST',
                    body: dto,
                }),
                invalidatesTags: ['CostComponent'],
            }),
            addCostComponentCalc: builder.mutation<{ costComponentCalcId: string }, CostComponentCalcDto>({
                query: (dto) => ({
                    url: '/workEffort/createCostComponentCalc',
                    method: 'POST',
                    body: dto,
                }),
                invalidatesTags: ['CostComponentCalc'],
            }),
            updateCostComponentCalc: builder.mutation<{ costComponentCalcId: string }, CostComponentCalcDto>({
                query: (dto) => ({
                    url: '/workEffort/updateCostComponentCalc',
                    method: 'POST',
                    body: dto,
                }),
                invalidatesTags: ['CostComponentCalc'],
            }),
            getWorkEffortCostCalcs: builder.query<WorkEffortCostCalcDto[], string>({
                query: (workEffortId) => `/workEffort/getWorkEffortCostCalcs/${workEffortId}`,
                providesTags: ['WorkEffortCostCalc'],
            }),
            createWorkEffortCostCalc: builder.mutation<{ workEffortId: string }, WorkEffortCostCalcDto>({
                query: (dto) => ({
                    url: '/workEffort/createWorkEffortCostCalc',
                    method: 'POST',
                    body: dto,
                }),
                invalidatesTags: ['WorkEffortCostCalc'],
            }),
            updateWorkEffortCostCalc: builder.mutation<{ workEffortId: string }, WorkEffortCostCalcDto>({
                query: (dto) => ({
                    url: '/workEffort/updateWorkEffortCostCalc',
                    method: 'PUT',
                    body: dto,
                }),
                invalidatesTags: ['WorkEffortCostCalc'],
            }),
            deleteWorkEffortCostCalc: builder.mutation<{ workEffortId: string }, { workEffortId: string; costComponentCalcId: string; costComponentTypeId: string; fromDate: string }>({
                query: (costComponent) => ({
                    url: `/workEffort/deleteWorkEffortCostCalc`,
                    method: 'DELETE',
                    body: costComponent,
                }),
                invalidatesTags: ['WorkEffortCostCalc'],
            }),
        };
    },
});

export const {
    useFetchCostComponentCalcsQuery,
    useFetchProductCostComponentCalcsQuery,
    useFetchMaterialCostQuery,
    useLazyFetchMaterialCostQuery,
    useFetchFixedAssetCostQuery,
    useFetchRoutingCostQuery,
    useFetchCostComponentsQuery,
    useFetchSimulatedBomCostQuery,
    useCalculateProductCostsMutation,
    useFetchActualCostComponentsQuery,
    useFetchLaborCostQuery,
    useLazyFetchLaborCostQuery,
    useFetchFOHCostQuery,
    useLazyFetchFOHCostQuery, useAddCostComponentCalcMutation,
    useUpdateCostComponentCalcMutation,
    useGetWorkEffortCostCalcsQuery,
    useCreateWorkEffortCostCalcMutation,
    useUpdateWorkEffortCostCalcMutation,
    useDeleteWorkEffortCostCalcMutation, useAddCostComponentMutation
} = costsApi;
export {costsApi};
