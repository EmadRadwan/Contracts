import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {ProductPromo} from "../../models/product/productPromo";

const availableProductPromotionsApi = createApi({
    reducerPath: "availableProductPromotions",
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
            fetchAvailableProductPromotions: builder.query<ProductPromo[], any>({
                query: (productId) => {
                    return {
                        url: `/products/${productId}/getAvailableProductPromotions`,
                        params: productId,
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta) => {
                    return response as ProductPromo[];
                },
            }),
        };
    },
});

export const {useFetchAvailableProductPromotionsQuery} =
    availableProductPromotionsApi;
export {availableProductPromotionsApi};
