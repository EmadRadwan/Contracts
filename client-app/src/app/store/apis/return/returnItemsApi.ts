import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {ReturnItem} from "../../../models/order/returnItem";
import {setUiNewReturnItems} from "../../../../features/orders/slice/returnUiSlice";

const returnItemsApi = createApi({
    reducerPath: "returnItems",
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

    tagTypes: ["SalesReturnItems", "PurchaseReturnItems"],
    endpoints(builder) {
        return {
            fetchSalesReturnItems: builder.query<ReturnItem[], any>({
                query: (returnId) => {
                    return {
                        url: `/returns/${returnId}/getSalesReturnItems`,
                        params: returnId,
                        method: "GET",
                    };
                },
                keepUnusedDataFor: 1,
                async onQueryStarted(id, {dispatch, queryFulfilled}) {
                    // `onStart` side-effect
                    try {
                        const {data} = await queryFulfilled;
                        // `onSuccess` side-effect
                        dispatch(setUiNewReturnItems(data));
                    } catch (err) {
                        // `onError` side-effect
                        //dispatch(messageCreated('Error fetching post!'))
                    }
                },
                providesTags: ["SalesReturnItems"],
                transformResponse: (response: any, meta, arg) => {
                    console.log("returnItems from Api", response);

                    return response as ReturnItem[];
                },
            }),
            fetchPurchaseReturnItems: builder.query<ReturnItem[], any>({
                query: (orderId) => {
                    console.log("queryArgs order item", orderId);
                    return {
                        url: `/orders/${orderId}/getPurchaseReturnItems`,
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
                        dispatch(setUiNewReturnItems(data));
                    } catch (err) {
                        // `onError` side-effect
                        //dispatch(messageCreated('Error fetching post!'))
                    }
                },
                providesTags: ["PurchaseReturnItems"],
                transformResponse: (response: any, meta, arg) => {
                    console.log("returnItems from Api", response);

                    return response as ReturnItem[];
                },
            }),
            addPurchaseReturnItems: builder.mutation({
                query: (returnItems) => {
                    return {
                        url: "/orders/createPurchaseReturnItems",
                        method: "POST",
                        body: {...returnItems},
                    };
                },
            }),
            addSalesReturnItems: builder.mutation({
                query: (returnItems) => {
                    return {
                        url: "/orders/createSalesReturnItems",
                        method: "POST",
                        body: {...returnItems},
                    };
                },
                invalidatesTags: ["SalesReturnItems"],
            }),
            updateSalesReturnItems: builder.mutation({
                query: (returnItems) => {
                    return {
                        url: "/orders/updateOrApproveSalesReturnItems",
                        method: "PUT",
                        body: {...returnItems},
                    };
                },
            }),
            updatePurchaseReturnItems: builder.mutation({
                query: (returnItems) => {
                    return {
                        url: "/orders/updatePurchaseReturnItems",
                        method: "PUT",
                        body: {...returnItems},
                    };
                },
            }),
            approveSalesReturnItems: builder.mutation({
                query: (returnItems) => {
                    return {
                        url: "/orders/updateOrApproveSalesReturnItems",
                        method: "PUT",
                        body: {...returnItems},
                    };
                },
            }),
            approvePurchaseReturnItems: builder.mutation({
                query: (returnItems) => {
                    return {
                        url: "/orders/approveSalesReturnItems",
                        method: "PUT",
                        body: {...returnItems},
                    };
                },
            }),
        };
    },
});

export const {
    useFetchSalesReturnItemsQuery,
    useFetchPurchaseReturnItemsQuery,
    useAddPurchaseReturnItemsMutation,
    useAddSalesReturnItemsMutation,
    useUpdateSalesReturnItemsMutation,
    useApproveSalesReturnItemsMutation,
    useApprovePurchaseReturnItemsMutation,
    useUpdatePurchaseReturnItemsMutation,
} = returnItemsApi;
export {returnItemsApi};
