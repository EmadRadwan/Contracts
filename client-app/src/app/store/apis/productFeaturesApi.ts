import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";

const productFeaturesApi = createApi({
    reducerPath: "productFeature",
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
            fetchProductFeatureColors: builder.query<ProductFeatureColor[], { language: string }>({
                query: () => ({
                    url: '/productFeatures/colors',
                    method: 'GET',
                }),
            }), 
            fetchProductFeatureTrademarks: builder.query<ProductFeatureColor[], { language: string }>({
                query: () => ({
                    url: '/productFeatures/trademarks',
                    method: 'GET',
                }),
            }), 
            fetchProductFeatureSizes: builder.query<ProductFeatureColor[], { language: string }>({
                query: () => ({
                    url: '/productFeatures/sizes',
                    method: 'GET',
                }),
            }),
        };
    },
});

export const {useFetchProductFeatureColorsQuery, useFetchProductFeatureTrademarksQuery, useFetchProductFeatureSizesQuery
} = productFeaturesApi;
export {productFeaturesApi};
