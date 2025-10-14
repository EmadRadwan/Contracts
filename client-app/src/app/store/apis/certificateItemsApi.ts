import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {CertificateItem} from "../../models/project/certificateItem";
import {
    resetUiCertificateItems,
    setCertificateItemsError,
    setUiCertificateItemsFromApi
} from "../../../features/Projects/slice/certificateItemsUiSlice";

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
            fetchCertificateItems: builder.query<CertificateItem[], string>({
                query: (workEffortId) => {
                    // REFACTOR: Validate workEffortId
                    // Purpose: Prevent invalid API calls
                    // Context: Avoids fetching with undefined or empty workEffortId
                    if (!workEffortId) {
                        throw new Error('workEffortId is required');
                    }
                    return {
                        url: `/project/${workEffortId}/getCertificateItems`,
                        method: 'GET',
                    };
                },
                keepUnusedDataFor: 1,
                async onQueryStarted(id, { dispatch, queryFulfilled }) {
                    // REFACTOR: Clear Redux store before fetching
                    // Purpose: Ensure no stale items remain in the Redux store before new data is fetched
                    // Context: Prevents components from rendering old items during fetch
                    dispatch(resetUiCertificateItems());

                    try {
                        const { data } = await queryFulfilled;
                        // REFACTOR: Update Redux store only after successful fetch
                        // Purpose: Ensure Redux store reflects the latest fetched data
                        // Context: Avoids partial updates if the fetch fails
                        dispatch(setUiCertificateItemsFromApi(data));
                    } catch (err) {
                        // REFACTOR: Handle fetch errors by dispatching an error action
                        // Purpose: Notify the application of fetch failures and prevent stale data
                        // Context: Allows UI to display an error state and avoids retaining old items
                        console.error("Error fetching certificate items:", err);
                        dispatch(setCertificateItemsError({ error: err.message || 'Failed to fetch certificate items' }));
                    }
                },
                providesTags: (result, error, workEffortId) => [
                    { type: 'CertificateItems', id: workEffortId },
                    'CertificateItems',
                ],
                transformResponse: (response: any) => {
                    return response.map((item: any) => ({
                        ...item,
                        id: item.id || `item-${item.workEffortId}-${Date.now()}`,
                        // REFACTOR: Add fallback id
                        // Purpose: Ensure every item has an id
                        // Context: Prevents TypeError in @react-pdf/renderer
                        achievementPercentage:
                            typeof item.achievementPercentage === 'number'
                                ? `${item.achievementPercentage}%`
                                : item.achievementPercentage || 'N/A',
                    })) as CertificateItem[];
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