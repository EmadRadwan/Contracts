import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../../configureStore";

const creditCardTypesApi = createApi({
    reducerPath: "creditCardTypes",
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

    endpoints (builder) {
        return {
            fetchCreditCardTypes: builder.query<any[], any>({
                query: () => {
                    return {
                        url: `/creditCardTypes`,
                        method: "GET"
                    }
                }
            })
        }
    }
})

export const {useFetchCreditCardTypesQuery } = creditCardTypesApi
export {creditCardTypesApi}