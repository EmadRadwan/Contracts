import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {setUiNewJobOrderAdjustments} from "../../../../features/orders/slice/jobOrderUiSlice";
import {store} from "../../configureStore";
import {OrderAdjustment} from "../../../models/order/orderAdjustment";

const jobOrderAdjustmentsApi = createApi({
    reducerPath: "jobOrderAdjustments",
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
            fetchJobOrderAdjustments: builder.query<OrderAdjustment[], any>({
                query: (orderId) => {
                    console.log("queryArgs order adjustment", orderId);
                    return {
                        url: `/jobOrders/${orderId}/getJobOrderAdjustments`,
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
                        dispatch(setUiNewJobOrderAdjustments(data));
                    } catch (err) {
                        // `onError` side-effect
                        //dispatch(messageCreated('Error fetching post!'))
                    }
                },
                transformResponse: (response: any, meta) => {
                    return response as OrderAdjustment[];
                },
            }),
        };
    },
});

export const {useFetchJobOrderAdjustmentsQuery} = jobOrderAdjustmentsApi;
export {jobOrderAdjustmentsApi};
