import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {setUiNewJobOrderItems} from "../../../../features/orders/slice/jobOrderUiSlice";
import {store} from "../../configureStore";
import {OrderItem} from "../../../models/order/orderItem";

const jobOrderItemsApi = createApi({
    reducerPath: "jobOrderItems",
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

    tagTypes: ["JobOrderItems"],
    endpoints(builder) {
        return {
            fetchJobOrderItems: builder.query<OrderItem[], any>({
                query: (orderId) => {
                    console.log("queryArgs order item", orderId);
                    return {
                        url: `/jobOrders/${orderId}/getJobOrderItems`,
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
                        dispatch(setUiNewJobOrderItems(data));
                    } catch (err) {
                        // `onError` side-effect
                        //dispatch(messageCreated('Error fetching post!'))
                    }
                },
                providesTags: ["JobOrderItems"],
                transformResponse: (response: any, meta, arg) => {
                    console.log("orderItems from Api", response);

                    return response as OrderItem[];
                },
            }),
        };
    },
});

export const {useFetchJobOrderItemsQuery} = jobOrderItemsApi;

export {jobOrderItemsApi};
