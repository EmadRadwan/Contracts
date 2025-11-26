import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";

const invoicePaymentApplicationsApi = createApi({
    reducerPath: "invoicePaymentApplications",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            return headers;
        },
    }),

    tagTypes: ["InvoicePaymentApplications"],
    endpoints(builder) {
        return {
            fetchInvoicePaymentApplications: builder.query<any[], { invoiceId: string }>({
                query: ({ invoiceId }) => ({
                    url: `/payments/${invoiceId}/getPaymentsApplicationsForInvoice`,
                    method: "GET",
                }),
            }),
        };
    },
});

export const {
    useFetchInvoicePaymentApplicationsQuery
} = invoicePaymentApplicationsApi;
export {invoicePaymentApplicationsApi};
