import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {State, toODataString} from "@progress/kendo-data-query";
import {InvoiceItemType} from "../../../models/accounting/invoiceItemType";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const invoiceItemTypesApi = createApi({
    reducerPath: "invoiceItemTypes",
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

    endpoints(builder) {
        return {
            fetchInvoiceItemTypes: builder.query<ListResponse<InvoiceItemType>, State>({
                query: (queryArgs) => {
                    const url = `/odata/invoiceItemTypeRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                transformResponse: (response: any, meta, arg) => {

                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            })
        }
    }
})

export const {useFetchInvoiceItemTypesQuery} = invoiceItemTypesApi
export {invoiceItemTypesApi}