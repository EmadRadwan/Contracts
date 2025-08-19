import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../configureStore";
import { ProductAssociation } from "../../models/product/productAssociation";

const productAssociationsApi = createApi({
    reducerPath: "productAssociations",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, { getState }) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            return headers;
        },
    }),
    tagTypes: ['ProductAssociations'],
    endpoints(builder) {
        return {
            addProductAssociation: builder.mutation({
                query: (association) => ({
                    url: "productAssociations/createProductAssociation",
                    method: "POST",
                    body: { ...association },
                }),
                invalidatesTags: ['ProductAssociations'],
            }),
            updateProductAssociation: builder.mutation({
                query: (association) => ({
                    url: "productAssociations/updateProductAssociation",
                    method: "PUT",
                    body: { ...association },
                }),
                invalidatesTags: ['ProductAssociations'],
            }),
            // REFACTOR: Enhanced tag invalidation for deleteProductAssociation to target specific cache entries
            // REFACTOR: Using specific tags based on productId to invalidate only relevant cache entries, improving performance
            deleteProductAssociation: builder.mutation<void, DeleteProductAssociationParams>({
                query: ({ productId, productIdTo, productAssocTypeId, fromDate }) => ({
                    url: `ProductAssociations`,
                    method: 'DELETE',
                    params: { productId, productIdTo, productAssocTypeId, fromDate },
                }),
                invalidatesTags: (result, error, { productId }) => [
                    { type: 'ProductAssociations', id: productId },
                    'ProductAssociations', // Fallback to invalidate all if needed
                ],
            }),
            fetchProductAssociations: builder.query<ProductAssociation[], string>({
                query: (productId) => `ProductAssociations?productId=${productId}`,
                // REFACTOR: Add caching to reduce API calls, aligning with typical OFBiz data stability
                keepUnusedDataFor: 300, // Cache for 5 minutes
                providesTags: (result, error, productId) =>
                    result
                        ? [
                            ...result.map(() => ({ type: 'ProductAssociations', id: productId })),
                            { type: 'ProductAssociations', id: 'LIST' },
                        ]
                        : [{ type: 'ProductAssociations', id: 'LIST' }],
            }),
        };
    },
});

export const {
    useAddProductAssociationMutation,
    useUpdateProductAssociationMutation,
    useDeleteProductAssociationMutation,
    useFetchProductAssociationsQuery,
} = productAssociationsApi;
export { productAssociationsApi };