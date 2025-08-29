import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {CertificateItem} from "../../models/project/certificateItem";
import {setUiCertificateItemsFromApi} from "../../../features/Projects/slice/certificateItemsUiSlice";

// REFACTOR: Define product LOV interface
// Purpose: Handle product details for certificate items with productId
// Context: Mirrors ProductLov from orderItemsApi.ts
interface ProductLov {
    productId: string;
    productName: string;
}

// REFACTOR: Define list response type
// Purpose: Wrap API response data
// Context: Matches ListProductLov from orderItemsApi.ts
interface ListProductLov<T> {
    data: T[];
}

// REFACTOR: Create RTK Query API
// Purpose: Manage certificate item endpoints
// Context: Modeled after orderItemsApi.ts, adapted for WorkEffort-based certificate items
const certificateItemsApi = createApi({
    reducerPath: "certificateItems",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // REFACTOR: Add auth and language headers
            // Purpose: Include token and language for API requests
            // Context: Matches orderItemsApi.ts
            const token = store.getState().account.user?.token;
            const lang = store.getState().localization.language;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            if (lang) {
                headers.set("Accept-Language", lang);
            }
            return headers;
        },
    }),
    tagTypes: ["CertificateItems"],
    endpoints(builder) {
        return {
            // REFACTOR: Fetch certificate items
            // Purpose: Retrieve items for a certificate by workEffortId
            // Context: Mirrors fetchPurchaseOrderItems
            fetchCertificateItems: builder.query<CertificateItem[], string>({
                query: (workEffortId) => ({
                    url: `/workEfforts/${workEffortId}/getCertificateItems`,
                    method: "GET",
                }),
                keepUnusedDataFor: 1,
                async onQueryStarted(id, {dispatch, queryFulfilled}) {
                    try {
                        const {data} = await queryFulfilled;
                        // REFACTOR: Update Redux store
                        // Purpose: Sync fetched items with certificateItemsUiSlice
                        // Context: Matches setUiOrderItemsFromApi
                        dispatch(setUiCertificateItemsFromApi(data));
                    } catch (err) {
                        console.error("Error fetching certificate items:", err);
                    }
                },
                providesTags: ["CertificateItems"],
                transformResponse: (response: any) => {
                    // REFACTOR: Transform response
                    // Purpose: Map API response to CertificateItem interface
                    // Context: Ensures compatibility with frontend
                    return response as CertificateItem[];
                },
            }),


            fetchCertificateItemProduct: builder.query<ListProductLov<ProductLov>, CertificateItem>({
                query: (certificateItem) => ({
                    url: `/workEfforts/${certificateItem.itemId}/getCertificateItemProduct`,
                    method: "GET",
                }),
                transformResponse: (response: any) => ({
                    data: response,
                }),
            }),

            // REFACTOR: Create certificate items
            // Purpose: Add new items to a certificate
            // Context: Mirrors addPurchaseOrderItems
            addCertificateItems: builder.mutation({
                query: (certificateItems) => ({
                    url: "/workEfforts/createCertificateItems",
                    method: "POST",
                    body: {...certificateItems},
                }),
                invalidatesTags: ["CertificateItems"],
            }),

            // REFACTOR: Update certificate items
            // Purpose: Update existing items
            // Context: Mirrors updatePurchaseOrderItems
            updateCertificateItems: builder.mutation({
                query: (certificateItems) => ({
                    url: "/workEfforts/updateCertificateItems",
                    method: "PUT",
                    body: {...certificateItems},
                }),
                invalidatesTags: ["CertificateItems"],
            }),

            // REFACTOR: Update or approve certificate items
            // Purpose: Handle update or approval in one endpoint
            // Context: Mirrors updateOrApproveSalesOrderItems
            updateOrApproveCertificateItems: builder.mutation({
                query: (certificateItems) => ({
                    url: "/workEfforts/updateOrApproveCertificateItems",
                    method: "PUT",
                    body: {...certificateItems},
                }),
                invalidatesTags: ["CertificateItems"],
            }),
        };
    },
});

// REFACTOR: Export hooks
// Purpose: Provide hooks for components to use
// Context: Matches orderItemsApi exports
export const {
    useFetchCertificateItemsQuery,
    useFetchCertificateItemProductQuery,
    useAddCertificateItemsMutation,
    useUpdateCertificateItemsMutation,
    useUpdateOrApproveCertificateItemsMutation,
} = certificateItemsApi;

export {certificateItemsApi};