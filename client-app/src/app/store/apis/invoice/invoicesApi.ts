import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../../configureStore";
import { Invoice } from "../../../models/accounting/invoice";
import { State, toODataString } from "@progress/kendo-data-query";
import {handleDatesObject} from "../../../util/utils";

interface ListResponse<T> {
  totalCount: number;
  data: T[];
}

const invoicesApi = createApi({
  reducerPath: "invoices",
  baseQuery: fetchBaseQuery({
    baseUrl: import.meta.env.VITE_API_URL,
    prepareHeaders: (headers, { getState }) => {
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

  tagTypes: ["invoices"],
  endpoints(builder) {
    return {
      fetchInvoices: builder.query<ListResponse<Invoice>, State>({
        query: (queryArgs) => {
          const url = `/odata/invoiceRecords?count=true&${toODataString(
            queryArgs
          )}`;
          return {
            url,
            method: "GET",
          };
        },
        providesTags: ["invoices"],
        transformResponse: (response: any, meta, arg) => {
          const { totalCount } = JSON.parse(
            meta!.response!.headers.get("count")!
          );
          return {
            data: response,
            totalCount: totalCount,
          };
        },
      }),
      fetchInvoiceTypes: builder.query<any[], any>({
        query: () => {
          return {
            url: "/invoiceTypes",
            method: "GET",
          };
        },
        transformResponse: (response: any, meta, arg) => {
          return response.filter(
            (i: any) =>
              i.parentTypeId === "PURCHASE_INVOICE" ||
              i.parentTypeId === "SALES_INVOICE" ||
              i.invoiceTypeId === "SALES_INVOICE" ||
              i.invoiceTypeId === "PURCHASE_INVOICE"
          );
        },
      }),
      calculateInvoiceTotal: builder.mutation<
        { id: string; total: number; outstandingAmount: number },
        string
      >({
        query: (invoiceId) => ({
          url: "/invoices/CalculateInvoiceTotal",
          method: "POST",
          body: { invoiceId }, // Send as JSON object to ensure valid JSON format
        }),
        // REFACTOR: Removed transformResponse since backend returns a single object
        // This simplifies the response handling for a single invoiceId
        providesTags: (result, error, invoiceId) => [
          { type: "InvoiceTotal", id: invoiceId },
        ],
      }),

      fetchNotListedInvoices: builder.query<any[], any>({
        query: (paymentId) => {
          return {
            url: `/invoices/${paymentId}/listNotAppliedInvoices`,
            method: "GET",
          };
        },
      }),
      createInvoice: builder.mutation({
        query: (invoice) => {
          return {
            url: "/invoices/createInvoice",
            method: "POST",
            body: { ...invoice },
          };
        },
        invalidatesTags: ["invoices"],
      }),
      updateInvoice: builder.mutation({
        query: (invoice) => {
          return {
            url: "/invoices/updateInvoice",
            method: "PUT",
            body: { ...invoice },
          };
        },
        invalidatesTags: (result, error, { invoiceId }) => [
          "invoices",
          { type: "invoices", id: invoiceId },
        ],
      }),
      changeInvoiceStatus: builder.mutation({
        query: ({
          invoiceId,
          statusId,
          paidDate,
          statusDate,
          actualCurrency,
        }) => {
          return {
            url: "/invoices/changeInvoiceStatus",
            method: "POST",
            body: { invoiceId, statusId, paidDate, statusDate, actualCurrency },
          };
        },
        invalidatesTags: (result, error, { invoiceId }) => [
          "invoices",
          { type: "invoices", id: invoiceId },
        ],
      }),
      fetchInvoiceStatusItems: builder.query<any[], any>({
        query: () => {
          return {
            url: "/invoices/getStatusItems",
            method: "GET",
          };
        },
      }),
      fetchInvoiceById: builder.query<Invoice, string>({
        query: (invoiceId) => `/invoices/${invoiceId}`,
        providesTags: (result, error, invoiceId) => [{ type: "invoices", id: invoiceId }],
      }),
    };
  },
});

export const {
  useFetchInvoicesQuery,
  useFetchInvoiceStatusItemsQuery,
  useFetchInvoiceTypesQuery,
  useFetchNotListedInvoicesQuery,
  useCreateInvoiceMutation,
  useUpdateInvoiceMutation,
  useCalculateInvoiceTotalMutation,
  useChangeInvoiceStatusMutation, useFetchInvoiceByIdQuery
} = invoicesApi;
export { invoicesApi };
