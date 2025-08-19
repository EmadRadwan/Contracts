import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {Facility} from "../../../models/facility/facility";
import { setOrdersForPickOrMoveStock } from "../../../../features/facilities/slice/facilityInventoryUiSlice";

const facilitiesApi = createApi({
    reducerPath: "facilities",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            const lang = store.getState().localization.language ;
            if (lang) {
                headers.set("Accept-Language", `${lang}`);
            }
            return headers;
        },
    }),
    tagTypes: ["PickList"],
    endpoints(builder) {
        return {
            fetchFacilities: builder.query<Facility[], undefined>({
                query: () => {
                    return {
                        url: "/facilities",
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta) => {
                    return response as Facility[];
                },
            }),
            fetchRowMaterialFacilities: builder.query<Facility[], undefined>({
                query: () => {
                    return {
                        url: "/facilities/getRowMaterialFacilities",
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta) => {
                    return response as Facility[];
                },
            }),
            fetchFinishedProductFacilities: builder.query<Facility[], undefined>({
                query: () => {
                    return {
                        url: "/facilities/getFinishedProductFacilities",
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta) => {
                    return response as Facility[];
                },
            }),
            fetchStockMovesNeeded: builder.query<any[], string>({
                query: (facilityId) => {
                    return {
                        url: "/facilities/findStockMovesNeeded",
                        method: "GET",
                        params: {facilityId}
                    };
                },
            }),
            fetchPicklistDisplayInfo: builder.query<any[], string>({
                query: (facilityId) => {
                    return {
                        url: "/facilities/getPicklistDisplayInfo",
                        method: "GET",
                        params: {facilityId}
                    };
                },
            }),
            fetchOrdersToPickMove: builder.query<any, string>({
                query: (facilityId) => {
                    return {
                        url: "/facilities/findOrdersToPickMove",
                        method: "GET",
                        params: {facilityId}
                    };
                },
                providesTags: ["PickList"],
                async onQueryStarted(id, {dispatch, queryFulfilled}) {
                    try {
                        const {data} = await queryFulfilled;
                        const orders = []
                        for (let group of data) {
                            if (group.orderReadyToPickInfoList.length > 0) {
                                for (let i = 0; i < group.orderReadyToPickInfoList.length; i++) {
                                    let order = {
                                        orderId: group.orderReadyToPickInfoList[i],
                                        needStockMove: "N",
                                        readyToPick: "Y"
                                    }
                                    orders.push(order)
                                }
                            }
                            if (group.orderNeedsStockMoveInfoList.length > 0) {
                                for (let i = 0; i < group.orderNeedsStockMoveInfoList.length; i++) {
                                    let order = {
                                        orderId: group.orderNeedsStockMoveInfoList[i],
                                        needStockMove: "Y",
                                        readyToPick: "N"
                                    }
                                    orders.push(order)
                                }
                            }
                        }
                        dispatch(setOrdersForPickOrMoveStock(orders));
                    } catch (err) {
                        
                    }
                }
            }),
            fetchRejectionReasons: builder.query<any[], undefined>({
                query: () => {
                    return {
                        url: "/rejectionReasons",
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta) => {
                    console.log("fetchRejectionReasons API has been called"); // Inserted console.log
                    return response;
                },
            }),
            fetchPackingData: builder.query<any, {
                facilityId: string;
                shipmentId?: string;
                orderId?: string;
                shipGroupSeqId?: string;
                picklistBinId?: string;
            }>({
                query: ({ facilityId, shipmentId, orderId, shipGroupSeqId, picklistBinId }) => ({
                    url: 'facilities/loadPackingData',
                    method: 'GET',
                    params: {
                        facilityId,
                        shipmentId,
                        orderId,
                        shipGroupSeqId,
                        picklistBinId
                    }
                }),
                keepUnusedDataFor: 60, // Cache data for 60 seconds
            }),

            createPickList: builder.mutation({
                query: (group) => {
                    return {
                        url: "/facilities/createPicklist",
                        method: "POST",
                        body: {...group}
                    }
                },
                invalidatesTags: ["PickList"]
            })
        };
    },
});

export const {useFetchFacilitiesQuery, useFetchRejectionReasonsQuery, useFetchRowMaterialFacilitiesQuery, useFetchFinishedProductFacilitiesQuery, useFetchOrdersToPickMoveQuery, useCreatePickListMutation,
    useFetchStockMovesNeededQuery,
    useFetchPicklistDisplayInfoQuery, useFetchPackingDataQuery} =
    facilitiesApi;
export {facilitiesApi};
