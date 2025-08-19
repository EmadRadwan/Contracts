import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";


const shipmentReceiptsApi = createApi({
    reducerPath: "shipmentReceipts",
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
            fetchShipmentReceipts: builder.query<any[], any>({
                query: (orderId) => {
                    // console.log("queryArgs order item", orderId)
                    return {
                        url: `/shipments/${orderId}/getShipmentReceipts`,
                        params: orderId,
                        method: "GET",
                    };
                },
            }),
        };
    },
});

export const {
    useFetchShipmentReceiptsQuery,
} = shipmentReceiptsApi;
export {shipmentReceiptsApi};
