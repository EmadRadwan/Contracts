import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {ProductType} from "../../models/product/productType";

const productTypesApi = createApi({
    reducerPath: "productTypes",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            const lang = store.getState().localization.language
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            if (lang) {
                headers.set("Accept-Language", lang)
            }
            return headers;
        },
    }),

    endpoints(builder) {
        return {
            fetchProductTypes: builder.query<ProductType[], undefined>({
                query: () => {
                    return {
                        url: "/productTypes",
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta) => {
                    return response as ProductType[];
                },
            }),
        };
    },
});

export const {useFetchProductTypesQuery} = productTypesApi;
export {productTypesApi};
