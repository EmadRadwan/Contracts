import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { ProductFacility } from "../../../../app/models/product/productFacility";

// REFACTOR: Updated API slice to align with provided structure, including VITE_API_URL and prepareHeaders.
// Added proper tagging with providesTags and invalidatesTags to manage cache invalidation for data consistency.
const productFacilitiesApi = createApi({
    reducerPath: 'productFacilities',
    baseQuery: fetchBaseQuery({
        baseUrl: `${import.meta.env.VITE_API_URL}/`,
        prepareHeaders: (headers, { getState }) => {
            // REFACTOR: Adopted provided prepareHeaders logic for authentication and localization.
            // Ensures token and language headers are set for all requests, maintaining consistency.
            const state = getState() as { account: { user?: { token?: string } }; localization: { language?: string } };
            const token = state.account.user?.token;
            const lang = state.localization.language;

            if (token) {
                headers.set('authorization', `Bearer ${token}`);
            }
            if (lang) {
                headers.set('Accept-Language', lang);
            }
            // REFACTOR: Ensured content-type is set to application/json for POST/PUT requests.
            headers.set('content-type', 'application/json');
            return headers;
        },
    }),
    // REFACTOR: Defined tagTypes to enable cache invalidation for ProductFacility data.
    tagTypes: ['ProductFacility'],
    endpoints: (builder) => ({
        createProductFacility: builder.mutation<ProductFacility, ProductFacility>({
            query: (productFacility) => ({
                // REFACTOR: Updated URL to match provided endpoint ('productFacilities').
                url: 'productFacilities',
                method: 'POST',
                body: productFacility,
            }),
            // REFACTOR: Added invalidatesTags to refresh the ProductFacility list after creating a new facility.
            // This ensures the getProductFacilities query is re-run to reflect the new data.
            invalidatesTags: ['ProductFacility'],
        }),
        updateProductFacility: builder.mutation<ProductFacility, ProductFacility>({
            query: (productFacility) => ({
                // REFACTOR: Updated URL to match provided endpoint ('productFacilities/updateProductFacility').
                url: 'productFacilities/updateProductFacility',
                method: 'PUT',
                body: productFacility,
            }),
            // REFACTOR: Added invalidatesTags to refresh the ProductFacility list after updating a facility.
            // This keeps the cache in sync with the updated data.
            invalidatesTags: ['ProductFacility'],
        }),
        getProductFacilities: builder.query<ProductFacility[], string>({
            query: (productId) => ({
                // REFACTOR: Updated URL to match provided endpoint (`productFacilities/${productId}`).
                url: `productFacilities/${productId}`,
                method: 'GET',
            }),
            // REFACTOR: Added providesTags to tag the query result with 'ProductFacility'.
            // This allows mutations to invalidate the cache and trigger a refetch when needed.
            providesTags: ['ProductFacility'],
        }),
    }),
});

// REFACTOR: Exported hooks for use in components, ensuring compatibility with existing code.
export const {
    useCreateProductFacilityMutation,
    useUpdateProductFacilityMutation,
    useGetProductFacilitiesQuery
} = productFacilitiesApi;

export { productFacilitiesApi };