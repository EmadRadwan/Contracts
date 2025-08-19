import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {Order} from "../../../models/order/order";
import {State, toODataString} from "@progress/kendo-data-query";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const jobOrdersApi = createApi({
    reducerPath: "jobOrders",
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
            fetchJobOrders: builder.query<ListResponse<Order>, State>({
                query: (queryArgs) => {
                    const url = `/odata/jobOrderRecords?$count=true&${toODataString(queryArgs)}`;
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
            updateOrApproveJobOrder: builder.mutation({
                query: (order) => {
                    return {
                        url: "/orders/updateOrApproveSalesOrder",
                        method: "PUT",
                        body: {...order},
                    };
                },
            }),
            completeJobOrder: builder.mutation({
                query: (order) => {
                    return {
                        url: "orders/completeSalesOrder",
                        method: "PUT",
                        body: {...order},
                    };
                },
            }),
        };
    },
});

export const {
    useFetchJobOrdersQuery,
    useUpdateOrApproveJobOrderMutation,
    useCompleteJobOrderMutation,
} = jobOrdersApi;
export {jobOrdersApi};
