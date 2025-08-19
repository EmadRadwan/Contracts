import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { ProductPrice } from '../../models/product/productPrice';

// Define the API slice for product prices
const productPricesApi = createApi({
    reducerPath: 'productPrices',
    baseQuery: fetchBaseQuery({
        baseUrl: `${import.meta.env.VITE_API_URL}/productprices`,
        prepareHeaders: (headers, { getState }) => {
            // REFACTOR: Use getState from RTK Query instead of direct store access for type safety and to avoid potential circular dependencies
            const state = getState() as { account: { user?: { token?: string } }; localization: { language?: string } };
            const token = state.account.user?.token;
            const lang = state.localization.language;

            if (token) {
                headers.set('authorization', `Bearer ${token}`);
            }
            if (lang) {
                headers.set('Accept-Language', lang);
            }
            // REFACTOR: Added content-type header to ensure JSON format for POST/PUT requests
            headers.set('content-type', 'application/json');
            return headers;
        },
    }),
    // REFACTOR: Added tagTypes to support cache invalidation with providesTags and invalidatesTags
    tagTypes: ['GetProductPrice'],
    endpoints: (builder) => ({
        // GET endpoint to fetch product prices by productId
        getProductPrices: builder.query<ProductPrice[], { productId: string }>({
            query: ({ productId }) => ({
                // REFACTOR: Corrected URL to match controller's [HttpGet("{productId}")] route
                url: `/${productId}`,
                method: 'GET',
            }),
            providesTags: ['GetProductPrice'],
        }),

        // PUT endpoint to update a product price
        updateProductPrice: builder.mutation<ProductPrice, ProductPrice>({
            query: (productPrice) => ({
                // REFACTOR: Corrected URL to match controller's [HttpPut("updateProductPrice")] route
                url: '/updateProductPrice',
                method: 'PUT',
                body: productPrice,
            }),
            invalidatesTags: ['ProductPrice'],
        }),

        // POST endpoint to create a new product price
        createProductPrice: builder.mutation<ProductPrice, ProductPrice>({
            query: (productPrice) => ({
                // REFACTOR: Corrected URL to match controller's [HttpPost] route at root
                url: '',
                method: 'POST',
                body: productPrice,
            }),
            invalidatesTags: ['GetProductPrice'],
        }),
    }),
});

// Export hooks for usage in components
export const {
    useGetProductPricesQuery,
    useUpdateProductPriceMutation,
    useCreateProductPriceMutation,
} = productPricesApi;
export {productPricesApi}