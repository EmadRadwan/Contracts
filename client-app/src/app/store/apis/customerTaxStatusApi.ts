import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {PartyTaxStatus} from "../../models/party/party";

const customerTaxStatusApi = createApi({
    reducerPath: "customerTaxStatus",
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
            fetchCustomerTaxStatus: builder.query<PartyTaxStatus, any>({
                query: (customerId) => {
                    return {
                        url: `/parties/${customerId}/getCustomerTaxStatus`,
                        params: customerId,
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta) => {
                    return response as PartyTaxStatus;
                },
            }),
        };
    },
});

export const {useFetchCustomerTaxStatusQuery} = customerTaxStatusApi;
export {customerTaxStatusApi};
