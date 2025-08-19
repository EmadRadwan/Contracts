import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../configureStore";
import { Order } from "../../models/order/order";
import { State, toODataString } from "@progress/kendo-data-query";
import { OrderPaymentPreference } from "../../models/order/orderPaymentPreference";
import { setUiOrderTermsFromApi } from "../../../features/orders/slice/orderTermsUiSlice";
import {GlAccount} from "../../models/accounting/globalGlSettings";

interface ListResponse<T> {
  data: T[];
  total: number;
}

const ordersApi = createApi({
  reducerPath: "salesOrders",
  baseQuery: fetchBaseQuery({
    baseUrl: import.meta.env.VITE_API_URL,
    prepareHeaders: (headers, { getState }) => {
      // By default, if we have a token in the store, let's use that for authenticated requests
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
  tagTypes: ["orders", "language", "PurchaseOrders", "BillingAccount"],

  endpoints(builder) {
    return {
      addPurchaseOrder: builder.mutation({
        query: (order) => {
          return {
            url: "/orders/createPurchaseOrder",
            method: "POST",
            body: { ...order },
          };
        },
        invalidatesTags: ["PurchaseOrders", "orders"],
      }),
      updatePurchaseOrder: builder.mutation({
        query: (order) => {
          return {
            url: "/orders/updateOrApprovePurchaseOrder",
            method: "PUT",
            body: { ...order },
          };
        },
        invalidatesTags: ["PurchaseOrders"],
      }),
      approvePurchaseOrder: builder.mutation({
        query: (order) => {
          return {
            url: "/orders/updateOrApprovePurchaseOrder",
            method: "PUT",
            body: { ...order },
          };
        },
        invalidatesTags: ["PurchaseOrders"],
      }),
      completePurchaseOrder: builder.mutation({
        query: (order) => {
          return {
            url: "/orders/completePurchaseOrder",
            method: "PUT",
            body: { ...order },
          };
        },
        invalidatesTags: ["PurchaseOrders"],
      }),
      fetchApprovedPurchaseOrders: builder.query<any[], { partyId?: string }>({
        query: ({ partyId }) => ({
          url: `/orders/getApprovedPurchaseOrders${partyId ? `?partyId=${partyId}` : ''}`,
          method: "GET",
        }),
        providesTags: ["PurchaseOrders"],
        transformResponse: (response: any, meta) => {
          return response;
        },
      }),

      fetchOrders: builder.query<ListResponse<Order>, State>({
        query: (queryArgs) => {
          const url = `/odata/OrderRecords?$count=true&${toODataString(
            queryArgs
          )}`;
          return { url, method: "GET" };
        },
        providesTags: ["orders", "PurchaseOrders"],
        transformResponse: (response: any, meta, arg) => {
          const { totalCount } = JSON.parse(
            meta!.response!.headers.get("count")!
          );
          return {
            data: response,
            total: totalCount,
          };
        },
      }),
      addSalesOrder: builder.mutation({
        query: (order) => {
          return {
            url: "/orders/createSalesOrder",
            method: "POST",
            body: { ...order },
          };
        },
        invalidatesTags: ["orders", "BillingAccount"],
      }),
      updateSalesOrder: builder.mutation({
        query: (order) => {
          return {
            url: "/orders/updateOrApproveSalesOrder",
            method: "PUT",
            body: { ...order },
          };
        },
        invalidatesTags: ["orders"],
      }),
      approveSalesOrder: builder.mutation({
        query: (order) => {
          return {
            url: "/orders/updateOrApproveSalesOrder",
            method: "PUT",
            body: { ...order },
          };
        },
        invalidatesTags: ["orders"],
      }),
      quickShipSalesOrder: builder.mutation({
        query: (order) => {
          return {
            url: "/orders/quickShipSalesOrder",
            method: "PUT",
            body: { ...order },
          };
        },
        invalidatesTags: ["orders"],
      }),
      fetchOrdersByPartyId: builder.query<any[], any>({
        query: (partyId) => {
          return {
            url: `/orders/${partyId}/getSalesOrdersByPartyId`,
            params: partyId,
            method: "GET",
          };
        },
      }),
      receiveOfflinePayment: builder.mutation({
        query: (offlinePayment) => {
          return {
            url: "/payments/receiveOfflinePayment",
            method: "POST",
            body: { ...offlinePayment },
          };
        },
      }),
      fetchOrderPaymentPreference: builder.query<OrderPaymentPreference[], any>(
        {
          query: (orderId) => {
            return {
              url: `/orders/${orderId}/getSalesOrderPaymentPreference`,
              method: "GET",
            };
          },
        }
      ),
      fetchOrderTerms: builder.query<any[], string>({
        query: (orderId) => {
          return {
            url: `/orders/${orderId}/getOrderTerms`,
            method: "GET",
          };
        },
        keepUnusedDataFor: 1,
        async onQueryStarted(id, { dispatch, queryFulfilled }) {
          // `onStart` side-effect
          try {
            const { data } = await queryFulfilled;
            // `onSuccess` side-effect
            dispatch(setUiOrderTermsFromApi(data));
          } catch (err) {
            // `onError` side-effect
            //dispatch(messageCreated('Error fetching post!'))
          }
        },
      }),
      fetchApprovedSalesOrders: builder.query({
        // REFACTOR: Add partyId as a query parameter
        // Purpose: Allows filtering sales orders by customer partyId
        // Context: Backend API expects partyId as a query string parameter
        query: ({ partyId }) => ({
          url: `/orders/getSalesOrdersApproved${partyId ? `?partyId=${partyId}` : ''}`,
          method: 'GET',
        }),
        providesTags: ['orders'],
        transformResponse: (response) => {
          return response;
        },
      }),
      fetchBillingAccountsByPartyId: builder.query<GlAccount[], any>({
        query: (partyId) => {
          return {
            url: `/billingAccounts/${partyId}/makePartyBillingAccountList`,
            method: "GET",
          };
        },
        providesTags: ["BillingAccount"],
      }),
      getBackOrderedQuantity: builder.query<any, string>({
        query: (orderId) => `/orders/getBackOrderedQuantity/${orderId}`,
        // REFACTOR: Transform response to match expected shape, handling success/failure from HandleResult
        transformResponse: (response: any) => {
          if (response.success) {
            return { success: true, data: response.value };
          }
          return { success: false, error: response.error || 'Failed to fetch back-ordered quantity' };
        },
        // REFACTOR: Add error handling for failed requests, improving robustness
        transformErrorResponse: (response: any) => {
          return { success: false, error: response.data?.error || 'An unexpected error occurred' };
        },
      }),
    };
  },
});

export const {
  useFetchOrdersQuery,
  useAddSalesOrderMutation,
  useUpdateSalesOrderMutation,
  useApproveSalesOrderMutation,
  useQuickShipSalesOrderMutation,
  useFetchOrdersByPartyIdQuery,
  useReceiveOfflinePaymentMutation,
  useFetchOrderPaymentPreferenceQuery,
  useAddPurchaseOrderMutation,
  useUpdatePurchaseOrderMutation,
  useApprovePurchaseOrderMutation,
  useCompletePurchaseOrderMutation,
  useFetchApprovedPurchaseOrdersQuery, useFetchApprovedSalesOrdersQuery, useFetchBillingAccountsByPartyIdQuery,
  useFetchOrderTermsQuery, useGetBackOrderedQuantityQuery
} = ordersApi;
export { ordersApi };
