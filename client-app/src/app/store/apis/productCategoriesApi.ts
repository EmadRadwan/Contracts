import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {ProductCategory} from "../../models/product/productCategory";

const productCategoriesApi = createApi({
    reducerPath: "productCategories",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = (getState() as any).account.user?.token;
            const lang = (getState() as any).localization.language;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            if (lang) {
                headers.set("Accept-Language", `${lang}`)
            }
            return headers;
        },
    }),
    tagTypes: ["ProductCategoryMember"],
    endpoints(builder) {
        return {
            fetchProductCategoriesFlat: builder.query<ProductCategory[], undefined>({
                query: () => {
                    return {
                        url: "/productCategories",
                        method: "GET",
                    };
                }
            }),
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
            fetchProductCategoryMembers: builder.query<ProductCategory[], string>({
                query: (productId) => {
                    return {
                        url: `/productCategories/${productId}`,
                        method: "GET",
                    };
                },
                providesTags: (result, error, productId) => [{ type: "ProductCategoryMember", id: productId }],
            }),
            createProductCategoryMember: builder.mutation<ProductCategory, any>({
                query: (productCategoryMember) => {
                    return {
                        url: "/productCategories",
                        method: "POST",
                        body: productCategoryMember,
                    };
                },
                invalidatesTags: (result, error, { productId }) => [{ type: "ProductCategoryMember", id: productId }],
            }),
            deleteProductCategoryMember: builder.mutation<void, {productCategoryId: string, productId: string, fromDate: string}>({
                query: ({productCategoryId, productId, fromDate}) => {
                    return {
                        url: `/productCategories/${productCategoryId}/${productId}/${fromDate}`,
                        method: "DELETE",
                    };
                },
                invalidatesTags: (result, error, { productId }) => [{ type: "ProductCategoryMember", id: productId }],
            }),
        };
    },
});

export const {
    useFetchProductCategoriesFlatQuery,
    useFetchProductCategoriesQuery,
    useFetchProductCategoriesRawMaterialsQuery,
    useFetchProductCategoryMembersQuery,
    useCreateProductCategoryMemberMutation,
    useDeleteProductCategoryMemberMutation
} = productCategoriesApi;
export {productCategoriesApi};
