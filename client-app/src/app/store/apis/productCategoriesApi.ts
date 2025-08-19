import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {ProductCategory} from "../../models/product/productCategory";

const productCategoriesApi = createApi({
    reducerPath: "productCategories",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            const lang = store.getState().localization.language;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            if (lang) {
                headers.set("Accept-Language", `${lang}`)
            }
            return headers;
        },
    }),

    endpoints(builder) {
        return {
            fetchProductCategories: builder.query<ProductCategory[], undefined>({
                query: () => {
                    return {
                        url: "/productCategories/getHierarchicalCategories",
                        method: "GET",
                    };
                }
            }),
            fetchProductCategoriesRawMaterials: builder.query<ProductCategory[], undefined>({
                query: () => {
                    return {
                        url: "/productCategories/getHierarchicalCategoriesRawMaterials",
                        method: "GET",
                    };
                }
            }),
        };
    },
});

export const {useFetchProductCategoriesQuery, useFetchProductCategoriesRawMaterialsQuery
} = productCategoriesApi;
export {productCategoriesApi};
