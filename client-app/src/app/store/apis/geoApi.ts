import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../configureStore";

const geoApi = createApi({
    reducerPath: "geo",
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
            fetchCountries: builder.query<any, any>({
                query: () => {
                    return {
                        url: "/geoCountry",
                        method: "GET"
                    }
                }
            })
        }
    }
})

export const {
    useFetchCountriesQuery
} = geoApi;
export {geoApi};