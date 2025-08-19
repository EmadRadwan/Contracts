import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {ProductLov} from "../../../models/product/productLov";
import {store} from "../../configureStore";
import {InvoiceItem} from "../../../models/accounting/invoiceItem";
import {setUiInvoiceItemsFromApi} from "../../../../features/accounting/invoice/slice/invoiceItemsUiSlice";

interface ListProductLov<T> {
    data: T[];
}

const invoiceItemsApi = createApi({
    reducerPath: "invoiceItems",
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

    tagTypes: ["InvoiceItems"],
    endpoints(builder) {
        return {
            fetchInvoiceItems: builder.query<InvoiceItem[], any>({
                query: (invoiceId) => {
                    // console.log("queryArgs invoice item", invoiceId)
                    return {
                        url: `/invoices/${invoiceId}/getInvoiceItems`,
                        params: invoiceId,
                        method: "GET",
                    };
                },
                keepUnusedDataFor: 1,
                async onQueryStarted(id, {dispatch, queryFulfilled}) {
                    // `onStart` side-effect
                    try {
                        const {data} = await queryFulfilled;
                        // `onSuccess` side-effect
                        // console.log("success", data);
                        dispatch(setUiInvoiceItemsFromApi(data));
                    } catch (err) {
                        // `onError` side-effect
                        // dispatch(messageCreated('Error fetching post!'))
                    }
                },
                providesTags: ["InvoiceItems"],
                transformResponse: (response: any, meta, arg) => {
                    // console.log("invoiceItems from Api", response)

                    return response as InvoiceItem[];
                },
            }),
            fetchInvoiceItemProduct: builder.query<ListProductLov<ProductLov>, any>({
                query: (invoiceItem) => {
                    const invoiceItemId =
                        invoiceItem.invoiceId + invoiceItem.invoiceItemSeqId;
                    console.count("fetchInvoiceItemProduct");
                    return {
                        url: `/invoices/${invoiceItemId}/getInvoiceItemProduct`,
                        params: invoiceItemId,
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta, arg) => {
                    return {
                        data: response,
                    };
                },
            }),
            addInvoiceItem: builder.mutation({
                query: (invoiceItem) => {
                    return {
                        url: "/invoices/createInvoiceItem",
                        method: "POST",
                        body: {...invoiceItem},
                    };
                },
                invalidatesTags: ["InvoiceItems"],
            }),
            updateInvoiceItems: builder.mutation({
                query: (invoiceItems) => {
                    return {
                        url: "/invoices/updateOrApproveInvoiceItems",
                        method: "PUT",
                        body: {...invoiceItems},
                    };
                },
            }),
            approveInvoiceItems: builder.mutation({
                query: (invoiceItems) => {
                    return {
                        url: "/invoices/updateOrApproveInvoiceItems",
                        method: "PUT",
                        body: {...invoiceItems},
                    };
                },
            }),
            fetchInvoiceItemTypes: builder.query<any, {
                invoiceTypeId: string;
            }>({
                query: ({ invoiceTypeId}) => {
                    if (!invoiceTypeId) {
                        throw new Error("invoiceTypeId and partyId are required");
                    }
                    return {
                        url: `/organizationGl/getInvoiceItemTypes`,
                        method: "GET",
                        params: {
                            invoiceTypeId,
                        }
                    };
                },
            }),
            fetchInvoiceItemTypesByInvoiceId: builder.query<any, {
                invoiceId: string;
            }>({
                query: ({ invoiceId }) => {
                    // REFACTOR: Retained validation for invoiceId
                    // Purpose: Ensures required parameter is provided before making the request
                    // Improvement: Prevents invalid API calls, aligns with controller validation
                    if (!invoiceId) {
                        throw new Error("invoiceId is required");
                    }
                    return {
                        url: `/Invoices/getInvoiceItemTypesByInvoiceId`,
                        method: "GET",
                        params: {
                            invoiceId
                        }
                    };
                },
            }),
        };
    },
});

export const {
    useFetchInvoiceItemsQuery,
    useFetchInvoiceItemProductQuery,
    useAddInvoiceItemMutation,
    useUpdateInvoiceItemsMutation,
    useApproveInvoiceItemsMutation, useFetchInvoiceItemTypesQuery, useFetchInvoiceItemTypesByInvoiceIdQuery
} = invoiceItemsApi;
export {invoiceItemsApi};
