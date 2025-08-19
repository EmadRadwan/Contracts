import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../configureStore";
import { OrderTerm } from "../../models/order/orderTerm";

const termTypesApi = createApi({
    reducerPath: "termTypes",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            const language = store.getState().localization.language
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            if (language) {
                headers.set("Accept-Language", language)
            }
            return headers;
        },
    }),
    endpoints(builder) {
        return {
            fetchTermTypes: builder.query<OrderTerm[], any>({
                query: () => {
                    return {
                        url: "/termTypes",
                        method: "GET"
                    }
                }
            })
        }
    }
})

export const {
    useFetchTermTypesQuery
} = termTypesApi;
export {termTypesApi};