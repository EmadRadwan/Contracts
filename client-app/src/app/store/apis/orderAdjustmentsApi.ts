import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {OrderAdjustment} from "../../models/order/orderAdjustment";
import {setUiOrderAdjustmentsFromApi} from "../../../features/orders/slice/orderAdjustmentsUiSlice";

const orderAdjustmentsApi = createApi({
    reducerPath: "orderAdjustments",
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
    endpoints(builder) {
        return {
            fetchOrderAdjustments: builder.query<OrderAdjustment[], any>({
                query: (orderId) => {
                    // console.log("queryArgs order adjustment", orderId)
                    return {
                        url: `/orders/${orderId}/getSalesOrderAdjustments`,
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
                        dispatch(setUiOrderAdjustmentsFromApi(data));
                    } catch (err) {
                        // `onError` side-effect
                        //dispatch(messageCreated('Error fetching post!'))
                    }
                },
                transformResponse: (response: any, meta) => {
                    return response as OrderAdjustment[];
                },
            }),
            addSalesOrderAdjustments: builder.mutation({
                query: (orderAdjustments) => {
                    return {
                        url: "/orders/createSalesOrderAdjustments",
                        method: "POST",
                        body: {...orderAdjustments},
                    };
                },
            }),
            updateSalesOrderAdjustments: builder.mutation({
                query: (orderAdjustments) => {
                    return {
                        url: "/orders/updateOrApproveSalesOrderAdjustments",
                        method: "PUT",
                        body: {...orderAdjustments},
                    };
                },
            }),
            approveSalesOrderAdjustments: builder.mutation({
                query: (orderAdjustments) => {
                    return {
                        url: "/orders/updateOrApproveSalesOrderAdjustments",
                        method: "PUT",
                        body: {...orderAdjustments},
                    };
                },
            }),
        };
    },
});

export const {
    useFetchOrderAdjustmentsQuery,
    useAddSalesOrderAdjustmentsMutation,
    useUpdateSalesOrderAdjustmentsMutation,
    useApproveSalesOrderAdjustmentsMutation,
} = orderAdjustmentsApi;
export {orderAdjustmentsApi};
