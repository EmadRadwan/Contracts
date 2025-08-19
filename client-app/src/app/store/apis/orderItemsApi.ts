import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {OrderItem} from "../../models/order/orderItem";
import {ProductLov} from "../../models/product/productLov";
import {setUiOrderItemsFromApi} from "../../../features/orders/slice/orderItemsUiSlice";
import {ReceiveInventoryRequest} from "./shipment/ReceiveInventoryRequest";

interface ListProductLov<T> {
    data: T[];
}

const orderItemsApi = createApi({
    reducerPath: "orderItems",
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

    tagTypes: ["SalesOrderItems", "PurchaseOrderItems"],
    endpoints(builder) {
        return {
            fetchSalesOrderItems: builder.query<OrderItem[], any>({
                query: (orderId) => {
                    // console.log("queryArgs order item", orderId)
                    return {
                        url: `/orders/${orderId}/getSalesOrderItems`,
                        params: orderId,
                        method: "GET",
                    };
                },
                keepUnusedDataFor: 1,
                async onQueryStarted(id, {dispatch, queryFulfilled}) {
                    // `onStart` side-effect
                    try {
                        const {data} = await queryFulfilled;
                        // `onSuccess` side-effect
                        dispatch(setUiOrderItemsFromApi(data));
                    } catch (err) {
                        // `onError` side-effect
                        //dispatch(messageCreated('Error fetching post!'))
                    }
                },
                providesTags: ["SalesOrderItems"],
                transformResponse: (response: any, meta, arg) => {
                    // console.log("orderItems from Api", response)

                    return response as OrderItem[];
                },
            }),
            fetchPurchaseOrderItems: builder.query<OrderItem[], any>({
                query: (orderId) => {
                    // console.log("queryArgs order item", orderId)
                    return {
                        url: `/orders/${orderId}/getPurchaseOrderItems`,
                        params: orderId,
                        method: "GET",
                    };
                },
                keepUnusedDataFor: 1,
                async onQueryStarted(id, {dispatch, queryFulfilled}) {
                    // `onStart` side-effect
                    try {
                        const {data} = await queryFulfilled;
                        // `onSuccess` side-effect
                        dispatch(setUiOrderItemsFromApi(data));
                    } catch (err) {
                        // `onError` side-effect
                        //dispatch(messageCreated('Error fetching post!'))
                    }
                },
                providesTags: ["PurchaseOrderItems"],
                transformResponse: (response: any, meta, arg) => {
                    console.log("fetchPurchaseOrderItems API has been called"); // Inserted console.log

                    return response as OrderItem[];
                },
            }),
            fetchOrderItemProduct: builder.query<ListProductLov<ProductLov>, any>({
                query: (orderItem) => {
                    const orderItemId = orderItem.orderId + orderItem.orderItemSeqId;
                    console.count("fetchOrderItemProduct");
                    return {
                        url: `/orders/${orderItemId}/getSalesOrderItemProduct`,
                        params: orderItemId,
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta, arg) => {
                    return {
                        data: response,
                    };
                },
            }),
            quickReceivePurchaseOrder: builder.mutation({
                query: (args) => {
                    return {
                        url: "/orders/quickReceivePurchaseOrder",
                        method: "POST",
                        body: {...args},
                    };
                },
            }),
            addPurchaseOrderItems: builder.mutation({
                query: (orderItems) => {
                    return {
                        url: "/orders/createPurchaseOrderItems",
                        method: "POST",
                        body: {...orderItems},
                    };
                },
                invalidatesTags: ["PurchaseOrderItems"],
            }),
            addSalesOrderItems: builder.mutation({
                query: (orderItems) => {
                    return {
                        url: "/orders/createSalesOrderItems",
                        method: "POST",
                        body: {...orderItems},
                    };
                },
                invalidatesTags: ["SalesOrderItems"],
            }),
            updateSalesOrderItems: builder.mutation({
                query: (orderItems) => {
                    return {
                        url: "/orders/updateOrApproveSalesOrderItems",
                        method: "PUT",
                        body: {...orderItems},
                    };
                },
            }),
            updatePurchaseOrderItems: builder.mutation({
                query: (orderItems) => {
                    return {
                        url: "/orders/updatePurchaseOrderItems",
                        method: "PUT",
                        body: {...orderItems},
                    };
                },
            }),
            approveSalesOrderItems: builder.mutation({
                query: (orderItems) => {
                    return {
                        url: "/orders/updateOrApproveSalesOrderItems",
                        method: "PUT",
                        body: {...orderItems},
                    };
                },
            }),
            approvePurchaseOrderItems: builder.mutation({
                query: (orderItems) => {
                    return {
                        url: "/orders/approveSalesOrderItems",
                        method: "PUT",
                        body: {...orderItems},
                    };
                },
            })
        };
    },
});

export const {
    useFetchSalesOrderItemsQuery,
    useFetchPurchaseOrderItemsQuery, 
    useFetchOrderItemProductQuery,
    useAddPurchaseOrderItemsMutation,
    useAddSalesOrderItemsMutation,
    useUpdateSalesOrderItemsMutation,
    useApproveSalesOrderItemsMutation,
    useApprovePurchaseOrderItemsMutation,
    useUpdatePurchaseOrderItemsMutation,
} = orderItemsApi;
export {orderItemsApi};
