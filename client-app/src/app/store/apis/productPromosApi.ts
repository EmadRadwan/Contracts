import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {ProductPromo} from "../../models/product/productPromo";

const productPromosApi = createApi({
    reducerPath: "productPromos",
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
            fetchProductPromos: builder.query<ProductPromo[], undefined>({
                query: () => {
                    return {
                        url: "/productPromos/getProductPromos",
                        method: "GET",
                    };
                } /*,
                transformResponse: (response: any, meta, arg) => {

                    const promos = handleDatesArray(response) as ProductPromo[];
                    return promos.map(promo => ({
                        ...promo,
                        startDate: new Date(promo.startDate),
                        endDate: new Date(promo.endDate),
                    }));
                }*/,
            }),
        };
    },
});

export const {useFetchProductPromosQuery} = productPromosApi;
export {productPromosApi};
