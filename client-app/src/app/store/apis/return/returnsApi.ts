import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {Return} from "../../../models/order/return";
import {ReturnHeaderType} from "../../../models/order/returnHeaderType";
import {State, toODataString} from "@progress/kendo-data-query";
import { ReturnableItem } from "../../../models/order/returnableOrderItem";

interface ListResponse<T> {
    data: T[];
    total: number;
}

interface CustomResponse {
    status: boolean
    message: string | null
    returnableItems: ReturnableItem[] | null
}

type StatusItemDto = {
    StatusId: string;
    Description: string;
};

const returnsApi = createApi({
    reducerPath: "returns",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            return headers;
        },
    }),
    tagTypes: ['Returns'],
    endpoints(builder) {
        return {
            fetchReturns: builder.query<ListResponse<Return>, State>({
                query: (queryArgs) => {
                    const url = `/odata/returnRecords?$count=true&${toODataString(queryArgs)}`;
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
                providesTags: ['Returns']
            }),
            fetchReturnHeaderTypes: builder.query<ReturnHeaderType[], undefined>({
                query: () => {
                    return {
                        url: "/returnHeaderTypes",
                        method: "GET",
                    };
                },
            }),
            fetchReturnReasons: builder.query<any[], undefined>({
                query: () => {
                    return {
                        url: "/returnReasons",
                        method: "GET",
                    };
                },
            }),
            fetchReturnTypes: builder.query<any[], undefined>({
                query: () => {
                    return {
                        url: "/returnTypes",
                        method: "GET",
                    };
                },
            }),
            fetchReturnStatusItems: builder.query<StatusItemDto[], { returnId?: string; returnHeaderType: string }>({
                query: ({ returnId, returnHeaderType }) => ({
                    url: "/returnStatus/listReturnItemsStatus",
                    method: "GET",
                    // REFACTOR: Pass empty string for returnId if undefined to align with backend
                    params: { returnId: returnId ?? "", returnHeaderType }
                }),
            }),
            fetchReturnItemStatusItems: builder.query<any[], undefined>({
                query: () => {
                    return {
                        url: "/returnStatus/listReturnItemsStatusLov",
                        method: "GET",
                    };
                },
            }),
            getPartyOrders: builder.query<PartyOrder[], { returnId: string }>({
                query: ({ returnId }) => ({
                    url: `/returns/${returnId}/getPartyOrders`,
                    method: 'GET',
                }),
                // REFACTOR: Transform response to match PartyOrder type
                // Purpose: Ensure backend DTO matches frontend model
                // Why: Aligns with OrderHeaderItemAndRolesDto structure
                transformResponse: (response: any[]) => response.map(item => ({
                    orderId: item.orderId,
                    orderDate: item.orderDate,
                    partyId: item.partyId,
                    roleTypeId: item.roleTypeId,
                    orderTypeId: item.orderTypeId,
                    statusId: item.statusId,
                    productId: item.productId,
                    quantity: Number(item.quantity),
                    unitPrice: Number(item.unitPrice),
                    itemDescription: item.itemDescription,
                    orderItemSeqId: item.orderItemSeqId,
                })),
            }),
            fetchReturnOrderSummary: builder.query({
                query: (orderId) => {
                    return {
                        url: `/returns/${orderId}/getOrderSummary`,
                        method: "GET"
                    }
                }
            }),
            getReturnableItems: builder.query<ReturnableItemsResult, string>({
                query: (orderId) => ({
                    url: `/returns/${orderId}/getReturnableItems`,
                    method: 'GET',
                }),
            }),
            addReturn: builder.mutation({
                query: (orderReturn) => {
                    return {
                        url: "/returns/create",
                        method: "POST",
                        body: {...orderReturn},
                    };
                },
                invalidatesTags: ['Returns']
            }),
            updateReturn: builder.mutation({
                query: (orderReturn) => {
                    return {
                        url: "/returns/updateOrApproveReturn",
                        method: "PUT",
                        body: {...orderReturn},
                    };
                },
                invalidatesTags: ['Returns']
            }),
            approveReturn: builder.mutation({
                query: (orderReturn) => {
                    return {
                        url: "/returns/updateOrApproveReturn",
                        method: "PUT",
                        body: {...orderReturn},
                    };
                },
                invalidatesTags: ['Returns']
            }),
            completeReturn: builder.mutation({
                query: (orderReturn) => {
                    return {
                        url: "/returns/completeReturn",
                        method: "PUT",
                        body: {...orderReturn},
                    };
                },
                invalidatesTags: ['Returns']
            }),
            processReturnItemsOrAdjustments: builder.mutation({
                query: (items) => {
                    return {
                        url: "/returns/processReturnItemsOrAdjustments",
                        method: "POST",
                        body: items,
                    };
                },
            }),
            fetchReturnById: builder.query<Return, string>({
                query: (returnId) => `/returns/${returnId}`,
                transformResponse: (response: {
                    returnId: string;
                    returnHeaderTypeId: string;
                    statusId: string;
                    fromPartyId: { partyId: string; partyName: string };
                    fromPartyName: string;
                    toPartyId: { partyId: string; partyName: string } | null;
                    toPartyName: string;
                    entryDate?: string | null;
                    statusDescription: string;
                    returnHeaderTypeDescription: string;
                    destinationFacilityId?: string;
                    currencyUomId?: string;
                    needsInventoryReceive?: string;
                }): Return => ({
                    // REFACTOR: Transform response to match Return interface
                    // Purpose: Map backend ReturnRecord to frontend Return type
                    // Why: Ensures compatibility with EditReturn form initialization
                    returnId: response.returnId,
                    returnHeaderTypeId: response.returnHeaderTypeId,
                    statusId: response.statusId,
                    fromPartyId: response.fromPartyId,
                    toPartyId: response.toPartyId || undefined, // Handle potential null from inner join
                    companyId: null, // Not provided in ReturnRecord, set to null
                    currencyUomId: response.currencyUomId,
                    entryDate: response.entryDate ? new Date(response.entryDate) : null,
                    needsInventoryReceive: response.needsInventoryReceive,
                    destinationFacilityId: response.destinationFacilityId
                })
            })
        };
    },
});

export const {
    useFetchReturnsQuery,
    useFetchReturnHeaderTypesQuery,
    useFetchReturnReasonsQuery,
    useFetchReturnTypesQuery,
    useFetchReturnStatusItemsQuery,
    useFetchReturnItemStatusItemsQuery,
    useGetPartyOrdersQuery,
    useFetchReturnOrderSummaryQuery,
    useGetReturnableItemsQuery,
    useAddReturnMutation,
    useUpdateReturnMutation,
    useApproveReturnMutation,
    useCompleteReturnMutation,
    useProcessReturnItemsOrAdjustmentsMutation, useFetchReturnByIdQuery
} = returnsApi;
export {returnsApi};
