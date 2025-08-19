import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {FacilityInventory, FacilityInventoryParams,} from "../../../models/facility/facilityInventory";
import {Product} from "../../../models/product/product";
import {InventoryItemDetail,} from "../../../models/facility/inventoryItemDetail";
import {State, toODataString} from "@progress/kendo-data-query";
import {InventoryTransfer} from "../../../models/facility/inventoryTransfer";
import {ReceiveInventoryRequest} from "../shipment/ReceiveInventoryRequest";
import {setUiOrderItemsFromApi} from "../../../../features/orders/slice/orderItemsUiSlice";
import {OrderItem} from "../../../models/order/orderItem";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const inventoriesApi = createApi({
    reducerPath: "inventories",
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
                headers.set("Accept-Language", lang)
            }
            return headers;
        },
    }),
    tagTypes: ["InventoryTransfer", "PurchaseOrderItemsForReceive", "InventoriesByInventoryItem"],
    endpoints(builder) {
        return {
            fetchFacilityInventoriesByProduct: builder.query<ListResponse<FacilityInventory>, State>({
                providesTags: ["InventoriesByProduct"],
                query: (queryArgs) => {
                    const url = `/odata/facilityInventoryRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                transformResponse: (response: any, meta, arg) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            fetchFacilityInventoriesByInventoryItem: builder.query<ListResponse<FacilityInventory>, State>({
                providesTags: ["InventoriesByInventoryItem"],
                query: (queryArgs) => {
                    const url = `/odata/facilityInventoryItemRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                transformResponse: (response: any, meta, arg) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            fetchInventoryTransfer: builder.query<ListResponse<InventoryTransfer>, State>({
                providesTags: ["InventoryTransfer"],
                query: (queryArgs) => {
                    const url = `/odata/inventoryTransferRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                transformResponse: (response: any, meta, arg) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            fetchFacilityInventoryItemProduct: builder.query<Product,
                FacilityInventoryParams>({
                query: (queryArgs) => {
                    return {
                        url: "/products/getFacilityInventoryItemProduct",
                        params: queryArgs,
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta) => {
                    console.log("fetchFacilityInventoryItemProduct API has been called"); // Inserted console.log
                    return response;
                },
            }),
            fetchFacilityInventoriesByInventoryItemDetails: builder.query<ListResponse<InventoryItemDetail>, State>({
                query: (queryArgs) => {
                    const url = `/odata/facilityInventoryItemDetailRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                transformResponse: (response: any, meta, arg) => {
                    console.log("fetchFacilityInventoriesByInventoryItemDetails API has been called"); // Inserted console.log

                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            receiveInventoryProduct: builder.mutation({
                invalidatesTags: ["PurchaseOrderItemsForReceive"],
                query: (order) => {
                    return {
                        url: "/facilityInventories/receiveInventoryProducts",
                        method: "POST",
                        body: {...order},
                    };
                },
                transformErrorResponse: (response, meta, arg) => {
                    console.log("receiveInventoryProduct API has been called"); // Inserted console.log

                    return response;
                },
            }),
            packOrder: builder.mutation({
                invalidatesTags: ["PackOrder"],
                query: (order) => {
                    return {
                        url: "/facilityInventories/packOrder",
                        method: "PUT",
                        body: {...order},
                    };
                },
                transformErrorResponse: (response, meta, arg) => {
                    console.log("packOrder API has been called");
                    return response;
                },
            }),
            addInventoryTransfer: builder.mutation({
                invalidatesTags: ["InventoryTransfer"],
                query: (inventoryTransfer) => {
                    return {
                        url: "/facilityInventories/createInventoryTransfer",
                        method: "POST",
                        body: {...inventoryTransfer},
                    };
                },
            }),
            updateInventoryTransfer: builder.mutation({
                invalidatesTags: ["InventoryTransfer"],
                query: (inventoryTransfer) => {
                    return {
                        url: "/facilityInventories/updateInventoryTransfer",
                        method: "PUT",
                        body: {...inventoryTransfer},
                    };
                },
            }),
            fetchInventoryItemLots: builder.query<{ lotId: string }[], { productId: string; workEffortId: string }>({
                query: ({productId, workEffortId}) => ({
                    url: "/facilityInventories/lots",
                    params: {productId, workEffortId},
                }),
            }),
            fetchPurchaseOrderItemsForReceive: builder.query<any, ReceiveInventoryRequest>({
                query: ({facilityId, purchaseOrderId}) => ({
                    url: `orders/listPurchaseOrderItemsForReceive`,
                    method: 'POST', // Change method from GET to POST
                    body: {facilityId, purchaseOrderId}, // Send parameters in the body for a POST request
                }),
                keepUnusedDataFor: 60, // Keep data for 60 seconds (adjust as needed)
                async onQueryStarted(args, {dispatch, queryFulfilled}) {
                    try {
                        const {data} = await queryFulfilled;
                        dispatch(setUiOrderItemsFromApi(data.purchaseOrderItems));
                    } catch (err) {
                        // Handle error (optional)
                        console.error('Error fetching purchase order items:', err);
                    }
                },
                providesTags: ['PurchaseOrderItemsForReceive'],
                transformResponse: (response: any, meta, arg) => {
                    console.log('fetchPurchaseOrderItems API has been called');
                    return response as OrderItem[];
                },
            }),
            createInventoryItem: builder.mutation<CreateInventoryItemResponse, CreateInventoryItemRequest>({
                invalidatesTags: ['InventoriesByInventoryItem', 'InventoriesByProduct'],
                query: (data) => ({
                    url: '/facilityInventories/createInventoryItem',
                    method: 'POST',
                    body: data,
                })
            }),
            updateInventoryItem: builder.mutation<UpdateInventoryItemResult, UpdateInventoryItemDto>({
                invalidatesTags: ['InventoriesByInventoryItem', 'InventoriesByProduct'],
                query: (body) => ({
                    url: 'facilityInventories/updateInventoryItem',
                    method: 'POST',
                    body,
                }),
            }),
            createPhysicalInventoryAndVariance: builder.mutation<string, PhysicalInventoryVarianceDto>({
                query: (dto) => ({
                    url: 'facilities/createPhysicalInventoryAndVariance',
                    method: 'POST',
                    body: dto,
                }),
                // REFACTOR: Added invalidatesTags to refresh related data after mutation.
                // This ensures frontend cache reflects backend changes (e.g., new PhysicalInventory).
                invalidatesTags: ['PhysicalInventory', 'InventoriesByProduct', 'InventoryAvailableByFacility', "InventoriesByInventoryItem"],
            }),
            fetchInventoryAvailableByFacility: builder.query({
                providesTags: ['InventoryAvailableByFacility'],
                query: ({ facilityId, productId }) => ({
                    url: `facilityInventories/getInventoryAvailableByFacility`,
                    params: { facilityId, productId },
                }),
                keepUnusedDataFor: 0, // 👈 clears cache immediately on unmount
            }),
        };
    },
});

export const {
    useFetchFacilityInventoriesByProductQuery, useCreateInventoryItemMutation, useCreatePhysicalInventoryAndVarianceMutation,
    useFetchFacilityInventoriesByInventoryItemQuery, useFetchInventoryAvailableByFacilityQuery,
    useFetchFacilityInventoryItemProductQuery,
    useFetchFacilityInventoriesByInventoryItemDetailsQuery,
    useFetchInventoryTransferQuery,
    useReceiveInventoryProductMutation: useReceiveInventoryProductsMutation,
    useAddInventoryTransferMutation, useFetchInventoryItemLotsQuery,
    useUpdateInventoryTransferMutation, useFetchPurchaseOrderItemsForReceiveQuery, usePackOrderMutation, useUpdateInventoryItemMutation,
} = inventoriesApi;
export {inventoriesApi};


interface CreateInventoryItemRequest {
    productId: string;
    lotId?: string;
    containerId?: string;
    facilityId: string;
    locationSeqId: string;
    currencyUomId?: string;
    datetimeReceived?: string;
    expireDate?: string;
}

interface CreateInventoryItemResponse {
    inventoryItemId: string;
}

interface UpdateInventoryItemDto {
    productId: string;
    lotId?: string;
    containerId?: string;
    facilityId: string;
    locationSeqId: string;
    currencyUomId?: string;
    datetimeReceived?: string;
    expireDate?: string;
}

interface UpdateInventoryItemResult {
    inventoryItemId: string;
}
