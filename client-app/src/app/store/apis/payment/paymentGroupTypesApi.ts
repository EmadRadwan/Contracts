import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import { store } from "../../configureStore";

const paymentGroupTypesApi = createApi({
    reducerPath: "paymentGroupTypes",
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
    endpoints(builder) {
        return {
            fetchPaymentGroupTypes: builder.query<any, any>({
                query: (queryArgs) => {
                    return {
                        url: "/paymentGroupTypes",
                        method: "GET",
                    };
                },
            })
        }
    }
})

export const {
    useFetchPaymentGroupTypesQuery
} = paymentGroupTypesApi;
export {paymentGroupTypesApi}